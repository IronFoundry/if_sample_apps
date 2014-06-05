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

//        private MongoCursorEnumerator<MongoMessage<T>> _enumerator;	// our cursor enumerator
        private IEnumerator<MongoMessage<T>> _enumerator;	// our cursor enumerator
        private bool _startedReading = false;						// initial query on an empty collection is a special case

        public MongoQueue(string connectionString, string databaseName, string queueName, long queueSize)
        {
            // our queue name will be the same as the message class
            var client = new MongoClient(connectionString);
            var database = client.GetServer().GetDatabase(databaseName);

            if (!database.CollectionExists(queueName))
            {
                try
                {
                    Console.WriteLine("Creating queue '{0}' size {1}", queueName, queueSize);

                    var options = CollectionOptions
                        // use a capped collection so space is pre-allocated and re-used
                        .SetCapped(true)
                        // we don't need the default _id index that MongoDB normally created automatically
                        .SetAutoIndexId(false)
                        // limit the size of the collection and pre-allocated the space to this number of bytes
                        .SetMaxSize(queueSize);

                    database.CreateCollection(queueName, options);
                }
                catch
                {
                    // assume that any exceptions are because the collection already exists ...
                }
            }

            // get the queue collection for our messages
            _queue = database.GetCollection<MongoMessage<T>>(queueName);

            // check if we already have a 'last read' position to start from
            _position = database.GetCollection("_queueIndex"); // What does this do if this collection doesn't exist?
            var last = _position.FindOneById(queueName);
            if (last != null)
                _lastId = last["last"].AsObjectId;

            _positionQuery = Query.EQ("_id", queueName);
        }

        public void Send(T message)
        {
            // sending a message is easy - we just insert it into the collection
            // it will be given a new sequential Id and also be written to the end (of the capped collection)
            _queue.Insert(new MongoMessage<T>(message));
        }

        public T Receive()
        {
            // for reading, we give the impression to the client that we provide a single message at a time
            // which means we maintain a cursor and enumerator in the background and hide it from the caller

            if (_enumerator == null)
                _enumerator = InitializeCursor();

            // there is no end when you need to sit and wait for messages to arrive
            while (true)
            {
                try
                {
                    // do we have a message waiting?
                    // this may block on the server for a few seconds but will return as soon as something is available
                    if (_enumerator.MoveNext())
                    {
                        // yes - record the current position and return it to the client
                        _startedReading = true;
                        _lastId = _enumerator.Current.Id;
                        _position.Update(_positionQuery, Update.Set("last", _lastId), UpdateFlags.Upsert, WriteConcern.Unacknowledged);
                        return _enumerator.Current.Message;
                    }

                    if (!_startedReading)
                    {
                        // for an empty collection, we'll need to re-query to be notified of new records
                        Thread.Sleep(500);
                        _enumerator.Dispose();
                        _enumerator = InitializeCursor();
                    }
                    else
                    {
                        // if the cursor is dead then we need to re-query, otherwise we just go back to iterating over it
                        
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
