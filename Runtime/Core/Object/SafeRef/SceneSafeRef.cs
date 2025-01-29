using PLUME.Sample.ProtoBurst.Unity;
using Unity.Collections;
using UnityEngine.SceneManagement;

namespace PLUME.Core.Object.SafeRef
{
    public class SceneSafeRef
    {
        public static readonly SceneSafeRef Null = new();

        public Scene Scene { get; }
        public SceneIdentifier Identifier { get; }

        public bool IsNull => Identifier.Equals(SceneIdentifier.Null);

        internal SceneSafeRef()
        {
            Scene = default;
            Identifier = SceneIdentifier.Null;
        }
        
        internal SceneSafeRef(Scene scene, Guid guid, FixedString512Bytes path)
        {
            Scene = scene;
            Identifier = new SceneIdentifier(guid, scene.name ?? "", path);
        }

        public bool Equals(SceneSafeRef other)
        {
            if (ReferenceEquals(null, other)) return false;
            if (ReferenceEquals(this, other)) return true;
            return Identifier.Equals(other.Identifier);
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != GetType()) return false;
            return Equals((SceneSafeRef)obj);
        }

        public override int GetHashCode()
        {
            return Identifier.GetHashCode();
        }
    }
}