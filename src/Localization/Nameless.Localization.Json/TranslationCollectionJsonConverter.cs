using System.Collections.ObjectModel;
using Nameless.Localization.Json.Schema;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Nameless.Localization.Json {

    public sealed class TranslationCollectionJsonConverter : JsonConverter {

        #region Public Override Methods

        public override bool CanConvert(Type objectType) =>
            typeof(TranslationCollection) == objectType ||
            typeof(TranslationCollection[]) == objectType ||
            typeof(IList<TranslationCollection>) == objectType ||
            typeof(List<TranslationCollection>) == objectType ||
            typeof(HashSet<TranslationCollection>) == objectType ||
            typeof(Collection<TranslationCollection>) == objectType;

        public override object? ReadJson(JsonReader reader, Type objectType, object? existingValue, JsonSerializer serializer) {
            if (reader.TokenType == JsonToken.Null) { return string.Empty; }
            if (reader.TokenType == JsonToken.String) { return serializer.Deserialize(reader, objectType); }

            var translationCollectionSet = new HashSet<TranslationCollection>();
            var translationCollections = JObject.Load(reader).Values<JProperty>().ToArray();
            foreach (var translationCollection in translationCollections) {
                if (translationCollection == null) { continue; }

                var translationSet = new HashSet<Translation>();
                var translationJObjects = translationCollection.Values<JObject>().Where(_ => _ != null).ToArray();
                var translations = translationJObjects!.Values<JProperty>().ToArray();
                foreach (var translation in translations) {
                    if (translation == null) { continue; }

                    var valueJObjects = translation.Values<JToken>().Where(_ => _ != null).ToArray();
                    var values = valueJObjects!.Values<string>().ToArray();
                    translationSet.Add(new Translation(key: translation.Name, values: values!));
                }
                translationCollectionSet.Add(new TranslationCollection(translationCollection.Name, translationSet.ToArray()));
            }
            return Convert(translationCollectionSet, objectType);
        }

        public override void WriteJson(JsonWriter writer, object? value, JsonSerializer serializer) {
            throw new NotImplementedException();
        }

        #endregion

        #region Private Static Methods

        private static object Convert(ISet<TranslationCollection> messageCollectionSet, Type resultType) {
            if (resultType == typeof(TranslationCollection[])) {
                return messageCollectionSet.ToArray();
            }

            if (resultType == typeof(IList<TranslationCollection>) || resultType == typeof(List<TranslationCollection>)) {
                return messageCollectionSet.ToList();
            }

            if (resultType == typeof(HashSet<TranslationCollection>)) {
                return messageCollectionSet;
            }

            return new Collection<TranslationCollection>(messageCollectionSet.ToList());
        }

        #endregion
    }
}
