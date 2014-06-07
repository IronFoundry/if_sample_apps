    
$(function () {
    $.connection.hub.logging = true;
    // Reference the auto-generated proxy for the hub.
    var resultsCollector = $.connection.resultsCollector;

    // Create a function that the hub can call back to display messages.
    resultsCollector.client.addNewMessageToPage = function (result) {
        // Add the message to the page.
        $('#results').append('<li><strong>' + htmlEncode(result) + '</strong></li>');
    };
    
    // Start the connection.
    $.connection.hub.start().done(function () {
        resultsCollector.server.sendResults();
    });
});

// This optional function html-encodes messages for display in the page.
function htmlEncode(value) {
    var encodedValue = $('<div />').text(value).html();
    return encodedValue;
}