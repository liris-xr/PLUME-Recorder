// <auto-generated>
//     Generated by the protocol buffer compiler.  DO NOT EDIT!
//     source: unity/identifiers.proto
// </auto-generated>
#pragma warning disable 1591, 0612, 3021, 8981
#region Designer generated code

using pb = global::Google.Protobuf;
using pbc = global::Google.Protobuf.Collections;
using pbr = global::Google.Protobuf.Reflection;
using scg = global::System.Collections.Generic;
namespace PLUME.Sample.Unity {

  /// <summary>Holder for reflection information generated from unity/identifiers.proto</summary>
  public static partial class IdentifiersReflection {

    #region Descriptor
    /// <summary>File descriptor for unity/identifiers.proto</summary>
    public static pbr::FileDescriptor Descriptor {
      get { return descriptor; }
    }
    private static pbr::FileDescriptor descriptor;

    static IdentifiersReflection() {
      byte[] descriptorData = global::System.Convert.FromBase64String(
          string.Concat(
            "Chd1bml0eS9pZGVudGlmaWVycy5wcm90bxIScGx1bWUuc2FtcGxlLnVuaXR5",
            "IkQKFEdhbWVPYmplY3RJZGVudGlmaWVyEhYKDmdhbWVfb2JqZWN0X2lkGAEg",
            "ASgJEhQKDHRyYW5zZm9ybV9pZBgCIAEoCSJoChNDb21wb25lbnRJZGVudGlm",
            "aWVyEhQKDGNvbXBvbmVudF9pZBgBIAEoCRI7CglwYXJlbnRfaWQYAiABKAsy",
            "KC5wbHVtZS5zYW1wbGUudW5pdHkuR2FtZU9iamVjdElkZW50aWZpZXIiKwoP",
            "QXNzZXRJZGVudGlmaWVyEgoKAmlkGAEgASgJEgwKBHBhdGgYAiABKAkilgEK",
            "D1NjZW5lSWRlbnRpZmllchIKCgJpZBgBIAEoCRIVCg1ydW50aW1lX2luZGV4",
            "GAIgASgJEgwKBG5hbWUYAyABKAkSDAoEcGF0aBgEIAEoCRITCgtidWlsZF9p",
            "bmRleBgFIAEoBRIvCgRtb2RlGAYgASgOMiEucGx1bWUuc2FtcGxlLnVuaXR5",
            "LkxvYWRTY2VuZU1vZGUqSQoNTG9hZFNjZW5lTW9kZRIaChZMT0FEX1NDRU5F",
            "X01PREVfU0lOR0xFEAASHAoYTE9BRF9TQ0VORV9NT0RFX0FERElUSVZFEAFC",
            "FaoCElBMVU1FLlNhbXBsZS5Vbml0eWIGcHJvdG8z"));
      descriptor = pbr::FileDescriptor.FromGeneratedCode(descriptorData,
          new pbr::FileDescriptor[] { },
          new pbr::GeneratedClrTypeInfo(new[] {typeof(global::PLUME.Sample.Unity.LoadSceneMode), }, null, new pbr::GeneratedClrTypeInfo[] {
            new pbr::GeneratedClrTypeInfo(typeof(global::PLUME.Sample.Unity.GameObjectIdentifier), global::PLUME.Sample.Unity.GameObjectIdentifier.Parser, new[]{ "GameObjectId", "TransformId" }, null, null, null, null),
            new pbr::GeneratedClrTypeInfo(typeof(global::PLUME.Sample.Unity.ComponentIdentifier), global::PLUME.Sample.Unity.ComponentIdentifier.Parser, new[]{ "ComponentId", "ParentId" }, null, null, null, null),
            new pbr::GeneratedClrTypeInfo(typeof(global::PLUME.Sample.Unity.AssetIdentifier), global::PLUME.Sample.Unity.AssetIdentifier.Parser, new[]{ "Id", "Path" }, null, null, null, null),
            new pbr::GeneratedClrTypeInfo(typeof(global::PLUME.Sample.Unity.SceneIdentifier), global::PLUME.Sample.Unity.SceneIdentifier.Parser, new[]{ "Id", "RuntimeIndex", "Name", "Path", "BuildIndex", "Mode" }, null, null, null, null)
          }));
    }
    #endregion

  }
  #region Enums
  public enum LoadSceneMode {
    [pbr::OriginalName("LOAD_SCENE_MODE_SINGLE")] Single = 0,
    [pbr::OriginalName("LOAD_SCENE_MODE_ADDITIVE")] Additive = 1,
  }

  #endregion

  #region Messages
  public sealed partial class GameObjectIdentifier : pb::IMessage<GameObjectIdentifier>
  #if !GOOGLE_PROTOBUF_REFSTRUCT_COMPATIBILITY_MODE
      , pb::IBufferMessage
  #endif
  {
    private static readonly pb::MessageParser<GameObjectIdentifier> _parser = new pb::MessageParser<GameObjectIdentifier>(() => new GameObjectIdentifier());
    private pb::UnknownFieldSet _unknownFields;
    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    [global::System.CodeDom.Compiler.GeneratedCode("protoc", null)]
    public static pb::MessageParser<GameObjectIdentifier> Parser { get { return _parser; } }

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    [global::System.CodeDom.Compiler.GeneratedCode("protoc", null)]
    public static pbr::MessageDescriptor Descriptor {
      get { return global::PLUME.Sample.Unity.IdentifiersReflection.Descriptor.MessageTypes[0]; }
    }

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    [global::System.CodeDom.Compiler.GeneratedCode("protoc", null)]
    pbr::MessageDescriptor pb::IMessage.Descriptor {
      get { return Descriptor; }
    }

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    [global::System.CodeDom.Compiler.GeneratedCode("protoc", null)]
    public GameObjectIdentifier() {
      OnConstruction();
    }

    partial void OnConstruction();

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    [global::System.CodeDom.Compiler.GeneratedCode("protoc", null)]
    public GameObjectIdentifier(GameObjectIdentifier other) : this() {
      gameObjectId_ = other.gameObjectId_;
      transformId_ = other.transformId_;
      _unknownFields = pb::UnknownFieldSet.Clone(other._unknownFields);
    }

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    [global::System.CodeDom.Compiler.GeneratedCode("protoc", null)]
    public GameObjectIdentifier Clone() {
      return new GameObjectIdentifier(this);
    }

    /// <summary>Field number for the "game_object_id" field.</summary>
    public const int GameObjectIdFieldNumber = 1;
    private string gameObjectId_ = "";
    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    [global::System.CodeDom.Compiler.GeneratedCode("protoc", null)]
    public string GameObjectId {
      get { return gameObjectId_; }
      set {
        gameObjectId_ = pb::ProtoPreconditions.CheckNotNull(value, "value");
      }
    }

    /// <summary>Field number for the "transform_id" field.</summary>
    public const int TransformIdFieldNumber = 2;
    private string transformId_ = "";
    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    [global::System.CodeDom.Compiler.GeneratedCode("protoc", null)]
    public string TransformId {
      get { return transformId_; }
      set {
        transformId_ = pb::ProtoPreconditions.CheckNotNull(value, "value");
      }
    }

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    [global::System.CodeDom.Compiler.GeneratedCode("protoc", null)]
    public override bool Equals(object other) {
      return Equals(other as GameObjectIdentifier);
    }

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    [global::System.CodeDom.Compiler.GeneratedCode("protoc", null)]
    public bool Equals(GameObjectIdentifier other) {
      if (ReferenceEquals(other, null)) {
        return false;
      }
      if (ReferenceEquals(other, this)) {
        return true;
      }
      if (GameObjectId != other.GameObjectId) return false;
      if (TransformId != other.TransformId) return false;
      return Equals(_unknownFields, other._unknownFields);
    }

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    [global::System.CodeDom.Compiler.GeneratedCode("protoc", null)]
    public override int GetHashCode() {
      int hash = 1;
      if (GameObjectId.Length != 0) hash ^= GameObjectId.GetHashCode();
      if (TransformId.Length != 0) hash ^= TransformId.GetHashCode();
      if (_unknownFields != null) {
        hash ^= _unknownFields.GetHashCode();
      }
      return hash;
    }

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    [global::System.CodeDom.Compiler.GeneratedCode("protoc", null)]
    public override string ToString() {
      return pb::JsonFormatter.ToDiagnosticString(this);
    }

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    [global::System.CodeDom.Compiler.GeneratedCode("protoc", null)]
    public void WriteTo(pb::CodedOutputStream output) {
    #if !GOOGLE_PROTOBUF_REFSTRUCT_COMPATIBILITY_MODE
      output.WriteRawMessage(this);
    #else
      if (GameObjectId.Length != 0) {
        output.WriteRawTag(10);
        output.WriteString(GameObjectId);
      }
      if (TransformId.Length != 0) {
        output.WriteRawTag(18);
        output.WriteString(TransformId);
      }
      if (_unknownFields != null) {
        _unknownFields.WriteTo(output);
      }
    #endif
    }

    #if !GOOGLE_PROTOBUF_REFSTRUCT_COMPATIBILITY_MODE
    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    [global::System.CodeDom.Compiler.GeneratedCode("protoc", null)]
    void pb::IBufferMessage.InternalWriteTo(ref pb::WriteContext output) {
      if (GameObjectId.Length != 0) {
        output.WriteRawTag(10);
        output.WriteString(GameObjectId);
      }
      if (TransformId.Length != 0) {
        output.WriteRawTag(18);
        output.WriteString(TransformId);
      }
      if (_unknownFields != null) {
        _unknownFields.WriteTo(ref output);
      }
    }
    #endif

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    [global::System.CodeDom.Compiler.GeneratedCode("protoc", null)]
    public int CalculateSize() {
      int size = 0;
      if (GameObjectId.Length != 0) {
        size += 1 + pb::CodedOutputStream.ComputeStringSize(GameObjectId);
      }
      if (TransformId.Length != 0) {
        size += 1 + pb::CodedOutputStream.ComputeStringSize(TransformId);
      }
      if (_unknownFields != null) {
        size += _unknownFields.CalculateSize();
      }
      return size;
    }

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    [global::System.CodeDom.Compiler.GeneratedCode("protoc", null)]
    public void MergeFrom(GameObjectIdentifier other) {
      if (other == null) {
        return;
      }
      if (other.GameObjectId.Length != 0) {
        GameObjectId = other.GameObjectId;
      }
      if (other.TransformId.Length != 0) {
        TransformId = other.TransformId;
      }
      _unknownFields = pb::UnknownFieldSet.MergeFrom(_unknownFields, other._unknownFields);
    }

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    [global::System.CodeDom.Compiler.GeneratedCode("protoc", null)]
    public void MergeFrom(pb::CodedInputStream input) {
    #if !GOOGLE_PROTOBUF_REFSTRUCT_COMPATIBILITY_MODE
      input.ReadRawMessage(this);
    #else
      uint tag;
      while ((tag = input.ReadTag()) != 0) {
        switch(tag) {
          default:
            _unknownFields = pb::UnknownFieldSet.MergeFieldFrom(_unknownFields, input);
            break;
          case 10: {
            GameObjectId = input.ReadString();
            break;
          }
          case 18: {
            TransformId = input.ReadString();
            break;
          }
        }
      }
    #endif
    }

    #if !GOOGLE_PROTOBUF_REFSTRUCT_COMPATIBILITY_MODE
    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    [global::System.CodeDom.Compiler.GeneratedCode("protoc", null)]
    void pb::IBufferMessage.InternalMergeFrom(ref pb::ParseContext input) {
      uint tag;
      while ((tag = input.ReadTag()) != 0) {
        switch(tag) {
          default:
            _unknownFields = pb::UnknownFieldSet.MergeFieldFrom(_unknownFields, ref input);
            break;
          case 10: {
            GameObjectId = input.ReadString();
            break;
          }
          case 18: {
            TransformId = input.ReadString();
            break;
          }
        }
      }
    }
    #endif

  }

  public sealed partial class ComponentIdentifier : pb::IMessage<ComponentIdentifier>
  #if !GOOGLE_PROTOBUF_REFSTRUCT_COMPATIBILITY_MODE
      , pb::IBufferMessage
  #endif
  {
    private static readonly pb::MessageParser<ComponentIdentifier> _parser = new pb::MessageParser<ComponentIdentifier>(() => new ComponentIdentifier());
    private pb::UnknownFieldSet _unknownFields;
    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    [global::System.CodeDom.Compiler.GeneratedCode("protoc", null)]
    public static pb::MessageParser<ComponentIdentifier> Parser { get { return _parser; } }

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    [global::System.CodeDom.Compiler.GeneratedCode("protoc", null)]
    public static pbr::MessageDescriptor Descriptor {
      get { return global::PLUME.Sample.Unity.IdentifiersReflection.Descriptor.MessageTypes[1]; }
    }

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    [global::System.CodeDom.Compiler.GeneratedCode("protoc", null)]
    pbr::MessageDescriptor pb::IMessage.Descriptor {
      get { return Descriptor; }
    }

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    [global::System.CodeDom.Compiler.GeneratedCode("protoc", null)]
    public ComponentIdentifier() {
      OnConstruction();
    }

    partial void OnConstruction();

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    [global::System.CodeDom.Compiler.GeneratedCode("protoc", null)]
    public ComponentIdentifier(ComponentIdentifier other) : this() {
      componentId_ = other.componentId_;
      parentId_ = other.parentId_ != null ? other.parentId_.Clone() : null;
      _unknownFields = pb::UnknownFieldSet.Clone(other._unknownFields);
    }

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    [global::System.CodeDom.Compiler.GeneratedCode("protoc", null)]
    public ComponentIdentifier Clone() {
      return new ComponentIdentifier(this);
    }

    /// <summary>Field number for the "component_id" field.</summary>
    public const int ComponentIdFieldNumber = 1;
    private string componentId_ = "";
    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    [global::System.CodeDom.Compiler.GeneratedCode("protoc", null)]
    public string ComponentId {
      get { return componentId_; }
      set {
        componentId_ = pb::ProtoPreconditions.CheckNotNull(value, "value");
      }
    }

    /// <summary>Field number for the "parent_id" field.</summary>
    public const int ParentIdFieldNumber = 2;
    private global::PLUME.Sample.Unity.GameObjectIdentifier parentId_;
    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    [global::System.CodeDom.Compiler.GeneratedCode("protoc", null)]
    public global::PLUME.Sample.Unity.GameObjectIdentifier ParentId {
      get { return parentId_; }
      set {
        parentId_ = value;
      }
    }

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    [global::System.CodeDom.Compiler.GeneratedCode("protoc", null)]
    public override bool Equals(object other) {
      return Equals(other as ComponentIdentifier);
    }

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    [global::System.CodeDom.Compiler.GeneratedCode("protoc", null)]
    public bool Equals(ComponentIdentifier other) {
      if (ReferenceEquals(other, null)) {
        return false;
      }
      if (ReferenceEquals(other, this)) {
        return true;
      }
      if (ComponentId != other.ComponentId) return false;
      if (!object.Equals(ParentId, other.ParentId)) return false;
      return Equals(_unknownFields, other._unknownFields);
    }

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    [global::System.CodeDom.Compiler.GeneratedCode("protoc", null)]
    public override int GetHashCode() {
      int hash = 1;
      if (ComponentId.Length != 0) hash ^= ComponentId.GetHashCode();
      if (parentId_ != null) hash ^= ParentId.GetHashCode();
      if (_unknownFields != null) {
        hash ^= _unknownFields.GetHashCode();
      }
      return hash;
    }

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    [global::System.CodeDom.Compiler.GeneratedCode("protoc", null)]
    public override string ToString() {
      return pb::JsonFormatter.ToDiagnosticString(this);
    }

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    [global::System.CodeDom.Compiler.GeneratedCode("protoc", null)]
    public void WriteTo(pb::CodedOutputStream output) {
    #if !GOOGLE_PROTOBUF_REFSTRUCT_COMPATIBILITY_MODE
      output.WriteRawMessage(this);
    #else
      if (ComponentId.Length != 0) {
        output.WriteRawTag(10);
        output.WriteString(ComponentId);
      }
      if (parentId_ != null) {
        output.WriteRawTag(18);
        output.WriteMessage(ParentId);
      }
      if (_unknownFields != null) {
        _unknownFields.WriteTo(output);
      }
    #endif
    }

    #if !GOOGLE_PROTOBUF_REFSTRUCT_COMPATIBILITY_MODE
    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    [global::System.CodeDom.Compiler.GeneratedCode("protoc", null)]
    void pb::IBufferMessage.InternalWriteTo(ref pb::WriteContext output) {
      if (ComponentId.Length != 0) {
        output.WriteRawTag(10);
        output.WriteString(ComponentId);
      }
      if (parentId_ != null) {
        output.WriteRawTag(18);
        output.WriteMessage(ParentId);
      }
      if (_unknownFields != null) {
        _unknownFields.WriteTo(ref output);
      }
    }
    #endif

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    [global::System.CodeDom.Compiler.GeneratedCode("protoc", null)]
    public int CalculateSize() {
      int size = 0;
      if (ComponentId.Length != 0) {
        size += 1 + pb::CodedOutputStream.ComputeStringSize(ComponentId);
      }
      if (parentId_ != null) {
        size += 1 + pb::CodedOutputStream.ComputeMessageSize(ParentId);
      }
      if (_unknownFields != null) {
        size += _unknownFields.CalculateSize();
      }
      return size;
    }

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    [global::System.CodeDom.Compiler.GeneratedCode("protoc", null)]
    public void MergeFrom(ComponentIdentifier other) {
      if (other == null) {
        return;
      }
      if (other.ComponentId.Length != 0) {
        ComponentId = other.ComponentId;
      }
      if (other.parentId_ != null) {
        if (parentId_ == null) {
          ParentId = new global::PLUME.Sample.Unity.GameObjectIdentifier();
        }
        ParentId.MergeFrom(other.ParentId);
      }
      _unknownFields = pb::UnknownFieldSet.MergeFrom(_unknownFields, other._unknownFields);
    }

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    [global::System.CodeDom.Compiler.GeneratedCode("protoc", null)]
    public void MergeFrom(pb::CodedInputStream input) {
    #if !GOOGLE_PROTOBUF_REFSTRUCT_COMPATIBILITY_MODE
      input.ReadRawMessage(this);
    #else
      uint tag;
      while ((tag = input.ReadTag()) != 0) {
        switch(tag) {
          default:
            _unknownFields = pb::UnknownFieldSet.MergeFieldFrom(_unknownFields, input);
            break;
          case 10: {
            ComponentId = input.ReadString();
            break;
          }
          case 18: {
            if (parentId_ == null) {
              ParentId = new global::PLUME.Sample.Unity.GameObjectIdentifier();
            }
            input.ReadMessage(ParentId);
            break;
          }
        }
      }
    #endif
    }

    #if !GOOGLE_PROTOBUF_REFSTRUCT_COMPATIBILITY_MODE
    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    [global::System.CodeDom.Compiler.GeneratedCode("protoc", null)]
    void pb::IBufferMessage.InternalMergeFrom(ref pb::ParseContext input) {
      uint tag;
      while ((tag = input.ReadTag()) != 0) {
        switch(tag) {
          default:
            _unknownFields = pb::UnknownFieldSet.MergeFieldFrom(_unknownFields, ref input);
            break;
          case 10: {
            ComponentId = input.ReadString();
            break;
          }
          case 18: {
            if (parentId_ == null) {
              ParentId = new global::PLUME.Sample.Unity.GameObjectIdentifier();
            }
            input.ReadMessage(ParentId);
            break;
          }
        }
      }
    }
    #endif

  }

  public sealed partial class AssetIdentifier : pb::IMessage<AssetIdentifier>
  #if !GOOGLE_PROTOBUF_REFSTRUCT_COMPATIBILITY_MODE
      , pb::IBufferMessage
  #endif
  {
    private static readonly pb::MessageParser<AssetIdentifier> _parser = new pb::MessageParser<AssetIdentifier>(() => new AssetIdentifier());
    private pb::UnknownFieldSet _unknownFields;
    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    [global::System.CodeDom.Compiler.GeneratedCode("protoc", null)]
    public static pb::MessageParser<AssetIdentifier> Parser { get { return _parser; } }

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    [global::System.CodeDom.Compiler.GeneratedCode("protoc", null)]
    public static pbr::MessageDescriptor Descriptor {
      get { return global::PLUME.Sample.Unity.IdentifiersReflection.Descriptor.MessageTypes[2]; }
    }

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    [global::System.CodeDom.Compiler.GeneratedCode("protoc", null)]
    pbr::MessageDescriptor pb::IMessage.Descriptor {
      get { return Descriptor; }
    }

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    [global::System.CodeDom.Compiler.GeneratedCode("protoc", null)]
    public AssetIdentifier() {
      OnConstruction();
    }

    partial void OnConstruction();

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    [global::System.CodeDom.Compiler.GeneratedCode("protoc", null)]
    public AssetIdentifier(AssetIdentifier other) : this() {
      id_ = other.id_;
      path_ = other.path_;
      _unknownFields = pb::UnknownFieldSet.Clone(other._unknownFields);
    }

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    [global::System.CodeDom.Compiler.GeneratedCode("protoc", null)]
    public AssetIdentifier Clone() {
      return new AssetIdentifier(this);
    }

    /// <summary>Field number for the "id" field.</summary>
    public const int IdFieldNumber = 1;
    private string id_ = "";
    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    [global::System.CodeDom.Compiler.GeneratedCode("protoc", null)]
    public string Id {
      get { return id_; }
      set {
        id_ = pb::ProtoPreconditions.CheckNotNull(value, "value");
      }
    }

    /// <summary>Field number for the "path" field.</summary>
    public const int PathFieldNumber = 2;
    private string path_ = "";
    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    [global::System.CodeDom.Compiler.GeneratedCode("protoc", null)]
    public string Path {
      get { return path_; }
      set {
        path_ = pb::ProtoPreconditions.CheckNotNull(value, "value");
      }
    }

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    [global::System.CodeDom.Compiler.GeneratedCode("protoc", null)]
    public override bool Equals(object other) {
      return Equals(other as AssetIdentifier);
    }

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    [global::System.CodeDom.Compiler.GeneratedCode("protoc", null)]
    public bool Equals(AssetIdentifier other) {
      if (ReferenceEquals(other, null)) {
        return false;
      }
      if (ReferenceEquals(other, this)) {
        return true;
      }
      if (Id != other.Id) return false;
      if (Path != other.Path) return false;
      return Equals(_unknownFields, other._unknownFields);
    }

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    [global::System.CodeDom.Compiler.GeneratedCode("protoc", null)]
    public override int GetHashCode() {
      int hash = 1;
      if (Id.Length != 0) hash ^= Id.GetHashCode();
      if (Path.Length != 0) hash ^= Path.GetHashCode();
      if (_unknownFields != null) {
        hash ^= _unknownFields.GetHashCode();
      }
      return hash;
    }

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    [global::System.CodeDom.Compiler.GeneratedCode("protoc", null)]
    public override string ToString() {
      return pb::JsonFormatter.ToDiagnosticString(this);
    }

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    [global::System.CodeDom.Compiler.GeneratedCode("protoc", null)]
    public void WriteTo(pb::CodedOutputStream output) {
    #if !GOOGLE_PROTOBUF_REFSTRUCT_COMPATIBILITY_MODE
      output.WriteRawMessage(this);
    #else
      if (Id.Length != 0) {
        output.WriteRawTag(10);
        output.WriteString(Id);
      }
      if (Path.Length != 0) {
        output.WriteRawTag(18);
        output.WriteString(Path);
      }
      if (_unknownFields != null) {
        _unknownFields.WriteTo(output);
      }
    #endif
    }

    #if !GOOGLE_PROTOBUF_REFSTRUCT_COMPATIBILITY_MODE
    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    [global::System.CodeDom.Compiler.GeneratedCode("protoc", null)]
    void pb::IBufferMessage.InternalWriteTo(ref pb::WriteContext output) {
      if (Id.Length != 0) {
        output.WriteRawTag(10);
        output.WriteString(Id);
      }
      if (Path.Length != 0) {
        output.WriteRawTag(18);
        output.WriteString(Path);
      }
      if (_unknownFields != null) {
        _unknownFields.WriteTo(ref output);
      }
    }
    #endif

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    [global::System.CodeDom.Compiler.GeneratedCode("protoc", null)]
    public int CalculateSize() {
      int size = 0;
      if (Id.Length != 0) {
        size += 1 + pb::CodedOutputStream.ComputeStringSize(Id);
      }
      if (Path.Length != 0) {
        size += 1 + pb::CodedOutputStream.ComputeStringSize(Path);
      }
      if (_unknownFields != null) {
        size += _unknownFields.CalculateSize();
      }
      return size;
    }

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    [global::System.CodeDom.Compiler.GeneratedCode("protoc", null)]
    public void MergeFrom(AssetIdentifier other) {
      if (other == null) {
        return;
      }
      if (other.Id.Length != 0) {
        Id = other.Id;
      }
      if (other.Path.Length != 0) {
        Path = other.Path;
      }
      _unknownFields = pb::UnknownFieldSet.MergeFrom(_unknownFields, other._unknownFields);
    }

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    [global::System.CodeDom.Compiler.GeneratedCode("protoc", null)]
    public void MergeFrom(pb::CodedInputStream input) {
    #if !GOOGLE_PROTOBUF_REFSTRUCT_COMPATIBILITY_MODE
      input.ReadRawMessage(this);
    #else
      uint tag;
      while ((tag = input.ReadTag()) != 0) {
        switch(tag) {
          default:
            _unknownFields = pb::UnknownFieldSet.MergeFieldFrom(_unknownFields, input);
            break;
          case 10: {
            Id = input.ReadString();
            break;
          }
          case 18: {
            Path = input.ReadString();
            break;
          }
        }
      }
    #endif
    }

    #if !GOOGLE_PROTOBUF_REFSTRUCT_COMPATIBILITY_MODE
    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    [global::System.CodeDom.Compiler.GeneratedCode("protoc", null)]
    void pb::IBufferMessage.InternalMergeFrom(ref pb::ParseContext input) {
      uint tag;
      while ((tag = input.ReadTag()) != 0) {
        switch(tag) {
          default:
            _unknownFields = pb::UnknownFieldSet.MergeFieldFrom(_unknownFields, ref input);
            break;
          case 10: {
            Id = input.ReadString();
            break;
          }
          case 18: {
            Path = input.ReadString();
            break;
          }
        }
      }
    }
    #endif

  }

  public sealed partial class SceneIdentifier : pb::IMessage<SceneIdentifier>
  #if !GOOGLE_PROTOBUF_REFSTRUCT_COMPATIBILITY_MODE
      , pb::IBufferMessage
  #endif
  {
    private static readonly pb::MessageParser<SceneIdentifier> _parser = new pb::MessageParser<SceneIdentifier>(() => new SceneIdentifier());
    private pb::UnknownFieldSet _unknownFields;
    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    [global::System.CodeDom.Compiler.GeneratedCode("protoc", null)]
    public static pb::MessageParser<SceneIdentifier> Parser { get { return _parser; } }

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    [global::System.CodeDom.Compiler.GeneratedCode("protoc", null)]
    public static pbr::MessageDescriptor Descriptor {
      get { return global::PLUME.Sample.Unity.IdentifiersReflection.Descriptor.MessageTypes[3]; }
    }

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    [global::System.CodeDom.Compiler.GeneratedCode("protoc", null)]
    pbr::MessageDescriptor pb::IMessage.Descriptor {
      get { return Descriptor; }
    }

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    [global::System.CodeDom.Compiler.GeneratedCode("protoc", null)]
    public SceneIdentifier() {
      OnConstruction();
    }

    partial void OnConstruction();

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    [global::System.CodeDom.Compiler.GeneratedCode("protoc", null)]
    public SceneIdentifier(SceneIdentifier other) : this() {
      id_ = other.id_;
      runtimeIndex_ = other.runtimeIndex_;
      name_ = other.name_;
      path_ = other.path_;
      buildIndex_ = other.buildIndex_;
      mode_ = other.mode_;
      _unknownFields = pb::UnknownFieldSet.Clone(other._unknownFields);
    }

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    [global::System.CodeDom.Compiler.GeneratedCode("protoc", null)]
    public SceneIdentifier Clone() {
      return new SceneIdentifier(this);
    }

    /// <summary>Field number for the "id" field.</summary>
    public const int IdFieldNumber = 1;
    private string id_ = "";
    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    [global::System.CodeDom.Compiler.GeneratedCode("protoc", null)]
    public string Id {
      get { return id_; }
      set {
        id_ = pb::ProtoPreconditions.CheckNotNull(value, "value");
      }
    }

    /// <summary>Field number for the "runtime_index" field.</summary>
    public const int RuntimeIndexFieldNumber = 2;
    private string runtimeIndex_ = "";
    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    [global::System.CodeDom.Compiler.GeneratedCode("protoc", null)]
    public string RuntimeIndex {
      get { return runtimeIndex_; }
      set {
        runtimeIndex_ = pb::ProtoPreconditions.CheckNotNull(value, "value");
      }
    }

    /// <summary>Field number for the "name" field.</summary>
    public const int NameFieldNumber = 3;
    private string name_ = "";
    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    [global::System.CodeDom.Compiler.GeneratedCode("protoc", null)]
    public string Name {
      get { return name_; }
      set {
        name_ = pb::ProtoPreconditions.CheckNotNull(value, "value");
      }
    }

    /// <summary>Field number for the "path" field.</summary>
    public const int PathFieldNumber = 4;
    private string path_ = "";
    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    [global::System.CodeDom.Compiler.GeneratedCode("protoc", null)]
    public string Path {
      get { return path_; }
      set {
        path_ = pb::ProtoPreconditions.CheckNotNull(value, "value");
      }
    }

    /// <summary>Field number for the "build_index" field.</summary>
    public const int BuildIndexFieldNumber = 5;
    private int buildIndex_;
    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    [global::System.CodeDom.Compiler.GeneratedCode("protoc", null)]
    public int BuildIndex {
      get { return buildIndex_; }
      set {
        buildIndex_ = value;
      }
    }

    /// <summary>Field number for the "mode" field.</summary>
    public const int ModeFieldNumber = 6;
    private global::PLUME.Sample.Unity.LoadSceneMode mode_ = global::PLUME.Sample.Unity.LoadSceneMode.Single;
    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    [global::System.CodeDom.Compiler.GeneratedCode("protoc", null)]
    public global::PLUME.Sample.Unity.LoadSceneMode Mode {
      get { return mode_; }
      set {
        mode_ = value;
      }
    }

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    [global::System.CodeDom.Compiler.GeneratedCode("protoc", null)]
    public override bool Equals(object other) {
      return Equals(other as SceneIdentifier);
    }

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    [global::System.CodeDom.Compiler.GeneratedCode("protoc", null)]
    public bool Equals(SceneIdentifier other) {
      if (ReferenceEquals(other, null)) {
        return false;
      }
      if (ReferenceEquals(other, this)) {
        return true;
      }
      if (Id != other.Id) return false;
      if (RuntimeIndex != other.RuntimeIndex) return false;
      if (Name != other.Name) return false;
      if (Path != other.Path) return false;
      if (BuildIndex != other.BuildIndex) return false;
      if (Mode != other.Mode) return false;
      return Equals(_unknownFields, other._unknownFields);
    }

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    [global::System.CodeDom.Compiler.GeneratedCode("protoc", null)]
    public override int GetHashCode() {
      int hash = 1;
      if (Id.Length != 0) hash ^= Id.GetHashCode();
      if (RuntimeIndex.Length != 0) hash ^= RuntimeIndex.GetHashCode();
      if (Name.Length != 0) hash ^= Name.GetHashCode();
      if (Path.Length != 0) hash ^= Path.GetHashCode();
      if (BuildIndex != 0) hash ^= BuildIndex.GetHashCode();
      if (Mode != global::PLUME.Sample.Unity.LoadSceneMode.Single) hash ^= Mode.GetHashCode();
      if (_unknownFields != null) {
        hash ^= _unknownFields.GetHashCode();
      }
      return hash;
    }

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    [global::System.CodeDom.Compiler.GeneratedCode("protoc", null)]
    public override string ToString() {
      return pb::JsonFormatter.ToDiagnosticString(this);
    }

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    [global::System.CodeDom.Compiler.GeneratedCode("protoc", null)]
    public void WriteTo(pb::CodedOutputStream output) {
    #if !GOOGLE_PROTOBUF_REFSTRUCT_COMPATIBILITY_MODE
      output.WriteRawMessage(this);
    #else
      if (Id.Length != 0) {
        output.WriteRawTag(10);
        output.WriteString(Id);
      }
      if (RuntimeIndex.Length != 0) {
        output.WriteRawTag(18);
        output.WriteString(RuntimeIndex);
      }
      if (Name.Length != 0) {
        output.WriteRawTag(26);
        output.WriteString(Name);
      }
      if (Path.Length != 0) {
        output.WriteRawTag(34);
        output.WriteString(Path);
      }
      if (BuildIndex != 0) {
        output.WriteRawTag(40);
        output.WriteInt32(BuildIndex);
      }
      if (Mode != global::PLUME.Sample.Unity.LoadSceneMode.Single) {
        output.WriteRawTag(48);
        output.WriteEnum((int) Mode);
      }
      if (_unknownFields != null) {
        _unknownFields.WriteTo(output);
      }
    #endif
    }

    #if !GOOGLE_PROTOBUF_REFSTRUCT_COMPATIBILITY_MODE
    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    [global::System.CodeDom.Compiler.GeneratedCode("protoc", null)]
    void pb::IBufferMessage.InternalWriteTo(ref pb::WriteContext output) {
      if (Id.Length != 0) {
        output.WriteRawTag(10);
        output.WriteString(Id);
      }
      if (RuntimeIndex.Length != 0) {
        output.WriteRawTag(18);
        output.WriteString(RuntimeIndex);
      }
      if (Name.Length != 0) {
        output.WriteRawTag(26);
        output.WriteString(Name);
      }
      if (Path.Length != 0) {
        output.WriteRawTag(34);
        output.WriteString(Path);
      }
      if (BuildIndex != 0) {
        output.WriteRawTag(40);
        output.WriteInt32(BuildIndex);
      }
      if (Mode != global::PLUME.Sample.Unity.LoadSceneMode.Single) {
        output.WriteRawTag(48);
        output.WriteEnum((int) Mode);
      }
      if (_unknownFields != null) {
        _unknownFields.WriteTo(ref output);
      }
    }
    #endif

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    [global::System.CodeDom.Compiler.GeneratedCode("protoc", null)]
    public int CalculateSize() {
      int size = 0;
      if (Id.Length != 0) {
        size += 1 + pb::CodedOutputStream.ComputeStringSize(Id);
      }
      if (RuntimeIndex.Length != 0) {
        size += 1 + pb::CodedOutputStream.ComputeStringSize(RuntimeIndex);
      }
      if (Name.Length != 0) {
        size += 1 + pb::CodedOutputStream.ComputeStringSize(Name);
      }
      if (Path.Length != 0) {
        size += 1 + pb::CodedOutputStream.ComputeStringSize(Path);
      }
      if (BuildIndex != 0) {
        size += 1 + pb::CodedOutputStream.ComputeInt32Size(BuildIndex);
      }
      if (Mode != global::PLUME.Sample.Unity.LoadSceneMode.Single) {
        size += 1 + pb::CodedOutputStream.ComputeEnumSize((int) Mode);
      }
      if (_unknownFields != null) {
        size += _unknownFields.CalculateSize();
      }
      return size;
    }

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    [global::System.CodeDom.Compiler.GeneratedCode("protoc", null)]
    public void MergeFrom(SceneIdentifier other) {
      if (other == null) {
        return;
      }
      if (other.Id.Length != 0) {
        Id = other.Id;
      }
      if (other.RuntimeIndex.Length != 0) {
        RuntimeIndex = other.RuntimeIndex;
      }
      if (other.Name.Length != 0) {
        Name = other.Name;
      }
      if (other.Path.Length != 0) {
        Path = other.Path;
      }
      if (other.BuildIndex != 0) {
        BuildIndex = other.BuildIndex;
      }
      if (other.Mode != global::PLUME.Sample.Unity.LoadSceneMode.Single) {
        Mode = other.Mode;
      }
      _unknownFields = pb::UnknownFieldSet.MergeFrom(_unknownFields, other._unknownFields);
    }

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    [global::System.CodeDom.Compiler.GeneratedCode("protoc", null)]
    public void MergeFrom(pb::CodedInputStream input) {
    #if !GOOGLE_PROTOBUF_REFSTRUCT_COMPATIBILITY_MODE
      input.ReadRawMessage(this);
    #else
      uint tag;
      while ((tag = input.ReadTag()) != 0) {
        switch(tag) {
          default:
            _unknownFields = pb::UnknownFieldSet.MergeFieldFrom(_unknownFields, input);
            break;
          case 10: {
            Id = input.ReadString();
            break;
          }
          case 18: {
            RuntimeIndex = input.ReadString();
            break;
          }
          case 26: {
            Name = input.ReadString();
            break;
          }
          case 34: {
            Path = input.ReadString();
            break;
          }
          case 40: {
            BuildIndex = input.ReadInt32();
            break;
          }
          case 48: {
            Mode = (global::PLUME.Sample.Unity.LoadSceneMode) input.ReadEnum();
            break;
          }
        }
      }
    #endif
    }

    #if !GOOGLE_PROTOBUF_REFSTRUCT_COMPATIBILITY_MODE
    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    [global::System.CodeDom.Compiler.GeneratedCode("protoc", null)]
    void pb::IBufferMessage.InternalMergeFrom(ref pb::ParseContext input) {
      uint tag;
      while ((tag = input.ReadTag()) != 0) {
        switch(tag) {
          default:
            _unknownFields = pb::UnknownFieldSet.MergeFieldFrom(_unknownFields, ref input);
            break;
          case 10: {
            Id = input.ReadString();
            break;
          }
          case 18: {
            RuntimeIndex = input.ReadString();
            break;
          }
          case 26: {
            Name = input.ReadString();
            break;
          }
          case 34: {
            Path = input.ReadString();
            break;
          }
          case 40: {
            BuildIndex = input.ReadInt32();
            break;
          }
          case 48: {
            Mode = (global::PLUME.Sample.Unity.LoadSceneMode) input.ReadEnum();
            break;
          }
        }
      }
    }
    #endif

  }

  #endregion

}

#endregion Designer generated code