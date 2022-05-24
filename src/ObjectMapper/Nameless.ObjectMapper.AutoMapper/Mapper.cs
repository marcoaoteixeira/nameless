using AutoMapper;
using AutoMapper.Data;

namespace Nameless.ObjectMapper.AutoMapper {

    public sealed class Mapper : IMapper {

        #region Private Read-Only Fields

        private readonly Profile[] _profiles;

        #endregion

        #region Private Fields

        private global::AutoMapper.IMapper? _mapper;

        #endregion

        #region Public Constructors

        public Mapper(IEnumerable<Profile> profiles) {
            _profiles = (profiles ?? Enumerable.Empty<Profile>()).ToArray();

            Initialize();
        }

        #endregion

        #region Private Methods

        private void Initialize() {
            var config = new MapperConfiguration(cfg => {
                cfg.AddDataReaderMapping();

                foreach (var profile in _profiles) {
                    cfg.AddProfile(profile);
                }
            });
            _mapper = config.CreateMapper();
        }

        #endregion

        #region IMapper Members

        public object? Map(Type? from, Type? to, object? instance) {
            return _mapper!.Map(instance, from, to);
        }

        #endregion
    }
}
