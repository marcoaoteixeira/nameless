using System.Text;
using RabbitMQ.Client;

namespace Nameless.ProducerConsumer.RabbitMQ.Infrastructure;

[UnitTest]
public class JsonMessageSerializerTests {
    private sealed record Payload(string Name, int Value);

    [Fact]
    public void Serialize_ThenDeserialize_RoundTripsContentAndHeader() {
        // arrange
        var sut = new JsonMessageSerializer();
        var context = new ProducerContext {
            CorrelationId = "corr-1",
            MessageId = "msg-1",
            Timestamp = new AmqpTimestamp(1_700_000_000L)
        };

        // act
        var buffer = sut.Serialize(new Payload("alpha", 7), context);
        var message = sut.Deserialize<Payload>(buffer);

        // assert
        Assert.Multiple(
            () => Assert.Equal(new Payload("alpha", 7), message.Content),
            () => Assert.Equal("corr-1", message.Header.CorrelationID),
            () => Assert.Equal("msg-1", message.Header.MessageID),
            () => Assert.Equal(1_700_000_000L, message.Header.Timestamp)
        );
    }

    [Fact]
    public void Serialize_ReturnsUtf8Json() {
        // arrange
        var sut = new JsonMessageSerializer();

        // act
        var buffer = sut.Serialize("hello", new ProducerContext());
        var json = Encoding.UTF8.GetString(buffer);

        // assert
        Assert.Contains("\"Content\":\"hello\"", json);
    }

    [Fact]
    public void Serialize_WithNullContext_Throws() {
        // arrange
        var sut = new JsonMessageSerializer();

        // act & assert
        Assert.ThrowsAny<Exception>(() => sut.Serialize("hello", null!));
    }

    [Fact]
    public void Deserialize_WithJsonNullLiteral_ThrowsInvalidOperationException() {
        // arrange
        var sut = new JsonMessageSerializer();

        // act & assert
        Assert.Throws<InvalidOperationException>(() => sut.Deserialize<string>("null"u8.ToArray()));
    }

    [Fact]
    public void Deserialize_WithInvalidJson_Throws() {
        // arrange
        var sut = new JsonMessageSerializer();

        // act & assert
        Assert.ThrowsAny<Exception>(() => sut.Deserialize<string>("not-json"u8.ToArray()));
    }
}
