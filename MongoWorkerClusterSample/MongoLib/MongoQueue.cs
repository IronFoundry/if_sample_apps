using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using MongoDB.Bson;
using MongoDB.Driver;
using MongoDB.Driver.Builders;

// Mongo library code based on article here: http://captaincodeman.com/2011/05/28/simple-service-bus-message-queue-mongodb/
namespace MongoLib
{
    public interface IPublish<in T> where T : class
    {
        void Send(T message);
    }

    public interface ISubscribe<out T> where T : class
    {
        T Receive();
    }

    public class MongoMessage<T> where T : class
    {
        public ObjectId Id { get; private set; }
        public DateTime Enqueued { get; private set; }
        public T Message { get; private set; }

        public MongoMessage(T message)
        {
            Enqueued = DateTime.UtcNow;
            Message = message;
        }
    }

    public class MongoQueue<T> : IPublish<T>, ISubscribe<T> where T : class
    {
        private readonly MongoCollection<MongoMessage<T>> _queue;	// the collection for the messages
        private readonly MongoCollection<BsonDocument> _position;	// used to record the current position
        private readonly IMongoQuery _positionQuery;

        private ObjectId _lastId = ObjectId.Empty;					// the last _id read from the queue

        private IEnumerator<MongoMessage<T>> _enumerator;	// our cursor enumerator
        private bool _startedReading = false;						// initial query on an empty collection is a special case

        public MongoQueue(string connectionString, string databaseName, string queueName, long queueSize)
        {
            MongoClient client = new MongoClient(connectionString);
            var database = client.GetServer().GetDatabase(databaseName);

            if (!database.CollectionExists(queueName))
            {
                try
                {
                    Console.WriteLine("Creating queue '{0}' size {1}", queueName, queueSize);

                    var options = CollectionOptions
                        .SetCapped(true)
                        .SetAutoIndexId(false)
                        .SetMaxSize(queueSize);

                    database.CreateCollection(queueName, options);
                }
                catch
                {
                    // assume that any exceptions are because the collection already exists ...
                }
            }

            _queue = database.GetCollection<MongoMessage<T>>(queueName);

            _position = database.GetCollection("_queueIndex"); 
            var last = _position.FindOneById(queueName);
            if (last != null)
                _lastId = last["last"].AsObjectId;

            _positionQuery = Query.EQ("_id", queueName);
        }

        public void Send(T message)
        {
            _queue.Insert(new MongoMessage<T>(message));
        }

        public T Receive()
        {
            if (_enumerator == null)
                _enumerator = InitializeCursor();

            while (true)
            {
                try
                {
                    if (_enumerator.MoveNext())
                    {
                        _startedReading = true;
                        _lastId = _enumerator.Current.Id;
                        _position.Update(_positionQuery, Update.Set("last", _lastId), UpdateFlags.Upsert, WriteConcern.Unacknowledged);
                        return _enumerator.Current.Message;
                    }

                    if (!_startedReading)
                    {
                        Thread.Sleep(500);
                        _enumerator.Dispose();
                        _enumerator = InitializeCursor();
                    }
                    else
                    {
                        _enumerator.Dispose();
                        _enumerator = InitializeCursor();
                    }
                }
                catch (IOException)
                {
                    _enumerator.Dispose();
                    _enumerator = InitializeCursor();
                }
                catch (SocketException)
                {
                    _enumerator.Dispose();
                    _enumerator = InitializeCursor();
                }
            }
        }
        IEnumerator<MongoMessage<T>> InitializeCursor()
        {
            var cursor = _queue
                .Find(Query.GT("_id", _lastId))
                .SetFlags(
                    QueryFlags.AwaitData |
                    QueryFlags.NoCursorTimeout |
                    QueryFlags.TailableCursor
                )
                .SetSortOrder(SortBy.Ascending("$natural"));

            return cursor.GetEnumerator();
        }
    }
}
