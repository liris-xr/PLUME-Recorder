// <auto-generated>
//     Generated by the protocol buffer compiler.  DO NOT EDIT!
//     source: unity/scene.proto
// </auto-generated>
#pragma warning disable 1591, 0612, 3021, 8981
#region Designer generated code

using pb = global::Google.Protobuf;
using pbc = global::Google.Protobuf.Collections;
using pbr = global::Google.Protobuf.Reflection;
using scg = global::System.Collections.Generic;
namespace PLUME.Sample.Unity {

  /// <summary>Holder for reflection information generated from unity/scene.proto</summary>
  public static partial class SceneReflection {

    #region Descriptor
    /// <summary>File descriptor for unity/scene.proto</summary>
    public static pbr::FileDescriptor Descriptor {
      get { return descriptor; }
    }
    private static pbr::FileDescriptor descriptor;

    static SceneReflection() {
      byte[] descriptorData = global::System.Convert.FromBase64String(
          string.Concat(
            "ChF1bml0eS9zY2VuZS5wcm90bxIScGx1bWUuc2FtcGxlLnVuaXR5Ih0KD1Nj",
            "ZW5lSWRlbnRpZmllchIKCgJpZBgBIAEoCSKVAQoJTG9hZFNjZW5lEi8KAmlk",
            "GAEgASgLMiMucGx1bWUuc2FtcGxlLnVuaXR5LlNjZW5lSWRlbnRpZmllchIM",
            "CgRwYXRoGAIgASgJEhMKC2J1aWxkX2luZGV4GAMgASgFEjQKCWxvYWRfbW9k",
            "ZRgEIAEoDjIhLnBsdW1lLnNhbXBsZS51bml0eS5Mb2FkU2NlbmVNb2RlIj4K",
            "C1VubG9hZFNjZW5lEi8KAmlkGAEgASgLMiMucGx1bWUuc2FtcGxlLnVuaXR5",
            "LlNjZW5lSWRlbnRpZmllciJRChFDaGFuZ2VBY3RpdmVTY2VuZRI8Cg9hY3Rp",
            "dmVfc2NlbmVfaWQYASABKAsyIy5wbHVtZS5zYW1wbGUudW5pdHkuU2NlbmVJ",
            "ZGVudGlmaWVyKkkKDUxvYWRTY2VuZU1vZGUSGgoWTE9BRF9TQ0VORV9NT0RF",
            "X1NJTkdMRRAAEhwKGExPQURfU0NFTkVfTU9ERV9BRERJVElWRRABQhWqAhJQ",
            "TFVNRS5TYW1wbGUuVW5pdHliBnByb3RvMw=="));
      descriptor = pbr::FileDescriptor.FromGeneratedCode(descriptorData,
          new pbr::FileDescriptor[] { },
          new pbr::GeneratedClrTypeInfo(new[] {typeof(global::PLUME.Sample.Unity.LoadSceneMode), }, null, new pbr::GeneratedClrTypeInfo[] {
            new pbr::GeneratedClrTypeInfo(typeof(global::PLUME.Sample.Unity.SceneIdentifier), global::PLUME.Sample.Unity.SceneIdentifier.Parser, new[]{ "Id" }, null, null, null, null),
            new pbr::GeneratedClrTypeInfo(typeof(global::PLUME.Sample.Unity.LoadScene), global::PLUME.Sample.Unity.LoadScene.Parser, new[]{ "Id", "Path", "BuildIndex", "LoadMode" }, null, null, null, null),
            new pbr::GeneratedClrTypeInfo(typeof(global::PLUME.Sample.Unity.UnloadScene), global::PLUME.Sample.Unity.UnloadScene.Parser, new[]{ "Id" }, null, null, null, null),
            new pbr::GeneratedClrTypeInfo(typeof(global::PLUME.Sample.Unity.ChangeActiveScene), global::PLUME.Sample.Unity.ChangeActiveScene.Parser, new[]{ "ActiveSceneId" }, null, null, null, null)
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
      get { return global::PLUME.Sample.Unity.SceneReflection.Descriptor.MessageTypes[0]; }
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
      return Equals(_unknownFields, other._unknownFields);
    }

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    [global::System.CodeDom.Compiler.GeneratedCode("protoc", null)]
    public override int GetHashCode() {
      int hash = 1;
      if (Id.Length != 0) hash ^= Id.GetHashCode();
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
        }
      }
    }
    #endif

  }

  public sealed partial class LoadScene : pb::IMessage<LoadScene>
  #if !GOOGLE_PROTOBUF_REFSTRUCT_COMPATIBILITY_MODE
      , pb::IBufferMessage
  #endif
  {
    private static readonly pb::MessageParser<LoadScene> _parser = new pb::MessageParser<LoadScene>(() => new LoadScene());
    private pb::UnknownFieldSet _unknownFields;
    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    [global::System.CodeDom.Compiler.GeneratedCode("protoc", null)]
    public static pb::MessageParser<LoadScene> Parser { get { return _parser; } }

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    [global::System.CodeDom.Compiler.GeneratedCode("protoc", null)]
    public static pbr::MessageDescriptor Descriptor {
      get { return global::PLUME.Sample.Unity.SceneReflection.Descriptor.MessageTypes[1]; }
    }

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    [global::System.CodeDom.Compiler.GeneratedCode("protoc", null)]
    pbr::MessageDescriptor pb::IMessage.Descriptor {
      get { return Descriptor; }
    }

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    [global::System.CodeDom.Compiler.GeneratedCode("protoc", null)]
    public LoadScene() {
      OnConstruction();
    }

    partial void OnConstruction();

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    [global::System.CodeDom.Compiler.GeneratedCode("protoc", null)]
    public LoadScene(LoadScene other) : this() {
      id_ = other.id_ != null ? other.id_.Clone() : null;
      path_ = other.path_;
      buildIndex_ = other.buildIndex_;
      loadMode_ = other.loadMode_;
      _unknownFields = pb::UnknownFieldSet.Clone(other._unknownFields);
    }

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    [global::System.CodeDom.Compiler.GeneratedCode("protoc", null)]
    public LoadScene Clone() {
      return new LoadScene(this);
    }

    /// <summary>Field number for the "id" field.</summary>
    public const int IdFieldNumber = 1;
    private global::PLUME.Sample.Unity.SceneIdentifier id_;
    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    [global::System.CodeDom.Compiler.GeneratedCode("protoc", null)]
    public global::PLUME.Sample.Unity.SceneIdentifier Id {
      get { return id_; }
      set {
        id_ = value;
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

    /// <summary>Field number for the "build_index" field.</summary>
    public const int BuildIndexFieldNumber = 3;
    private int buildIndex_;
    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    [global::System.CodeDom.Compiler.GeneratedCode("protoc", null)]
    public int BuildIndex {
      get { return buildIndex_; }
      set {
        buildIndex_ = value;
      }
    }

    /// <summary>Field number for the "load_mode" field.</summary>
    public const int LoadModeFieldNumber = 4;
    private global::PLUME.Sample.Unity.LoadSceneMode loadMode_ = global::PLUME.Sample.Unity.LoadSceneMode.Single;
    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    [global::System.CodeDom.Compiler.GeneratedCode("protoc", null)]
    public global::PLUME.Sample.Unity.LoadSceneMode LoadMode {
      get { return loadMode_; }
      set {
        loadMode_ = value;
      }
    }

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    [global::System.CodeDom.Compiler.GeneratedCode("protoc", null)]
    public override bool Equals(object other) {
      return Equals(other as LoadScene);
    }

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    [global::System.CodeDom.Compiler.GeneratedCode("protoc", null)]
    public bool Equals(LoadScene other) {
      if (ReferenceEquals(other, null)) {
        return false;
      }
      if (ReferenceEquals(other, this)) {
        return true;
      }
      if (!object.Equals(Id, other.Id)) return false;
      if (Path != other.Path) return false;
      if (BuildIndex != other.BuildIndex) return false;
      if (LoadMode != other.LoadMode) return false;
      return Equals(_unknownFields, other._unknownFields);
    }

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    [global::System.CodeDom.Compiler.GeneratedCode("protoc", null)]
    public override int GetHashCode() {
      int hash = 1;
      if (id_ != null) hash ^= Id.GetHashCode();
      if (Path.Length != 0) hash ^= Path.GetHashCode();
      if (BuildIndex != 0) hash ^= BuildIndex.GetHashCode();
      if (LoadMode != global::PLUME.Sample.Unity.LoadSceneMode.Single) hash ^= LoadMode.GetHashCode();
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
      if (id_ != null) {
        output.WriteRawTag(10);
        output.WriteMessage(Id);
      }
      if (Path.Length != 0) {
        output.WriteRawTag(18);
        output.WriteString(Path);
      }
      if (BuildIndex != 0) {
        output.WriteRawTag(24);
        output.WriteInt32(BuildIndex);
      }
      if (LoadMode != global::PLUME.Sample.Unity.LoadSceneMode.Single) {
        output.WriteRawTag(32);
        output.WriteEnum((int) LoadMode);
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
      if (id_ != null) {
        output.WriteRawTag(10);
        output.WriteMessage(Id);
      }
      if (Path.Length != 0) {
        output.WriteRawTag(18);
        output.WriteString(Path);
      }
      if (BuildIndex != 0) {
        output.WriteRawTag(24);
        output.WriteInt32(BuildIndex);
      }
      if (LoadMode != global::PLUME.Sample.Unity.LoadSceneMode.Single) {
        output.WriteRawTag(32);
        output.WriteEnum((int) LoadMode);
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
      if (id_ != null) {
        size += 1 + pb::CodedOutputStream.ComputeMessageSize(Id);
      }
      if (Path.Length != 0) {
        size += 1 + pb::CodedOutputStream.ComputeStringSize(Path);
      }
      if (BuildIndex != 0) {
        size += 1 + pb::CodedOutputStream.ComputeInt32Size(BuildIndex);
      }
      if (LoadMode != global::PLUME.Sample.Unity.LoadSceneMode.Single) {
        size += 1 + pb::CodedOutputStream.ComputeEnumSize((int) LoadMode);
      }
      if (_unknownFields != null) {
        size += _unknownFields.CalculateSize();
      }
      return size;
    }

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    [global::System.CodeDom.Compiler.GeneratedCode("protoc", null)]
    public void MergeFrom(LoadScene other) {
      if (other == null) {
        return;
      }
      if (other.id_ != null) {
        if (id_ == null) {
          Id = new global::PLUME.Sample.Unity.SceneIdentifier();
        }
        Id.MergeFrom(other.Id);
      }
      if (other.Path.Length != 0) {
        Path = other.Path;
      }
      if (other.BuildIndex != 0) {
        BuildIndex = other.BuildIndex;
      }
      if (other.LoadMode != global::PLUME.Sample.Unity.LoadSceneMode.Single) {
        LoadMode = other.LoadMode;
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
            if (id_ == null) {
              Id = new global::PLUME.Sample.Unity.SceneIdentifier();
            }
            input.ReadMessage(Id);
            break;
          }
          case 18: {
            Path = input.ReadString();
            break;
          }
          case 24: {
            BuildIndex = input.ReadInt32();
            break;
          }
          case 32: {
            LoadMode = (global::PLUME.Sample.Unity.LoadSceneMode) input.ReadEnum();
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
            if (id_ == null) {
              Id = new global::PLUME.Sample.Unity.SceneIdentifier();
            }
            input.ReadMessage(Id);
            break;
          }
          case 18: {
            Path = input.ReadString();
            break;
          }
          case 24: {
            BuildIndex = input.ReadInt32();
            break;
          }
          case 32: {
            LoadMode = (global::PLUME.Sample.Unity.LoadSceneMode) input.ReadEnum();
            break;
          }
        }
      }
    }
    #endif

  }

  public sealed partial class UnloadScene : pb::IMessage<UnloadScene>
  #if !GOOGLE_PROTOBUF_REFSTRUCT_COMPATIBILITY_MODE
      , pb::IBufferMessage
  #endif
  {
    private static readonly pb::MessageParser<UnloadScene> _parser = new pb::MessageParser<UnloadScene>(() => new UnloadScene());
    private pb::UnknownFieldSet _unknownFields;
    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    [global::System.CodeDom.Compiler.GeneratedCode("protoc", null)]
    public static pb::MessageParser<UnloadScene> Parser { get { return _parser; } }

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    [global::System.CodeDom.Compiler.GeneratedCode("protoc", null)]
    public static pbr::MessageDescriptor Descriptor {
      get { return global::PLUME.Sample.Unity.SceneReflection.Descriptor.MessageTypes[2]; }
    }

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    [global::System.CodeDom.Compiler.GeneratedCode("protoc", null)]
    pbr::MessageDescriptor pb::IMessage.Descriptor {
      get { return Descriptor; }
    }

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    [global::System.CodeDom.Compiler.GeneratedCode("protoc", null)]
    public UnloadScene() {
      OnConstruction();
    }

    partial void OnConstruction();

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    [global::System.CodeDom.Compiler.GeneratedCode("protoc", null)]
    public UnloadScene(UnloadScene other) : this() {
      id_ = other.id_ != null ? other.id_.Clone() : null;
      _unknownFields = pb::UnknownFieldSet.Clone(other._unknownFields);
    }

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    [global::System.CodeDom.Compiler.GeneratedCode("protoc", null)]
    public UnloadScene Clone() {
      return new UnloadScene(this);
    }

    /// <summary>Field number for the "id" field.</summary>
    public const int IdFieldNumber = 1;
    private global::PLUME.Sample.Unity.SceneIdentifier id_;
    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    [global::System.CodeDom.Compiler.GeneratedCode("protoc", null)]
    public global::PLUME.Sample.Unity.SceneIdentifier Id {
      get { return id_; }
      set {
        id_ = value;
      }
    }

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    [global::System.CodeDom.Compiler.GeneratedCode("protoc", null)]
    public override bool Equals(object other) {
      return Equals(other as UnloadScene);
    }

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    [global::System.CodeDom.Compiler.GeneratedCode("protoc", null)]
    public bool Equals(UnloadScene other) {
      if (ReferenceEquals(other, null)) {
        return false;
      }
      if (ReferenceEquals(other, this)) {
        return true;
      }
      if (!object.Equals(Id, other.Id)) return false;
      return Equals(_unknownFields, other._unknownFields);
    }

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    [global::System.CodeDom.Compiler.GeneratedCode("protoc", null)]
    public override int GetHashCode() {
      int hash = 1;
      if (id_ != null) hash ^= Id.GetHashCode();
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
      if (id_ != null) {
        output.WriteRawTag(10);
        output.WriteMessage(Id);
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
      if (id_ != null) {
        output.WriteRawTag(10);
        output.WriteMessage(Id);
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
      if (id_ != null) {
        size += 1 + pb::CodedOutputStream.ComputeMessageSize(Id);
      }
      if (_unknownFields != null) {
        size += _unknownFields.CalculateSize();
      }
      return size;
    }

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    [global::System.CodeDom.Compiler.GeneratedCode("protoc", null)]
    public void MergeFrom(UnloadScene other) {
      if (other == null) {
        return;
      }
      if (other.id_ != null) {
        if (id_ == null) {
          Id = new global::PLUME.Sample.Unity.SceneIdentifier();
        }
        Id.MergeFrom(other.Id);
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
            if (id_ == null) {
              Id = new global::PLUME.Sample.Unity.SceneIdentifier();
            }
            input.ReadMessage(Id);
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
            if (id_ == null) {
              Id = new global::PLUME.Sample.Unity.SceneIdentifier();
            }
            input.ReadMessage(Id);
            break;
          }
        }
      }
    }
    #endif

  }

  public sealed partial class ChangeActiveScene : pb::IMessage<ChangeActiveScene>
  #if !GOOGLE_PROTOBUF_REFSTRUCT_COMPATIBILITY_MODE
      , pb::IBufferMessage
  #endif
  {
    private static readonly pb::MessageParser<ChangeActiveScene> _parser = new pb::MessageParser<ChangeActiveScene>(() => new ChangeActiveScene());
    private pb::UnknownFieldSet _unknownFields;
    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    [global::System.CodeDom.Compiler.GeneratedCode("protoc", null)]
    public static pb::MessageParser<ChangeActiveScene> Parser { get { return _parser; } }

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    [global::System.CodeDom.Compiler.GeneratedCode("protoc", null)]
    public static pbr::MessageDescriptor Descriptor {
      get { return global::PLUME.Sample.Unity.SceneReflection.Descriptor.MessageTypes[3]; }
    }

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    [global::System.CodeDom.Compiler.GeneratedCode("protoc", null)]
    pbr::MessageDescriptor pb::IMessage.Descriptor {
      get { return Descriptor; }
    }

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    [global::System.CodeDom.Compiler.GeneratedCode("protoc", null)]
    public ChangeActiveScene() {
      OnConstruction();
    }

    partial void OnConstruction();

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    [global::System.CodeDom.Compiler.GeneratedCode("protoc", null)]
    public ChangeActiveScene(ChangeActiveScene other) : this() {
      activeSceneId_ = other.activeSceneId_ != null ? other.activeSceneId_.Clone() : null;
      _unknownFields = pb::UnknownFieldSet.Clone(other._unknownFields);
    }

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    [global::System.CodeDom.Compiler.GeneratedCode("protoc", null)]
    public ChangeActiveScene Clone() {
      return new ChangeActiveScene(this);
    }

    /// <summary>Field number for the "active_scene_id" field.</summary>
    public const int ActiveSceneIdFieldNumber = 1;
    private global::PLUME.Sample.Unity.SceneIdentifier activeSceneId_;
    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    [global::System.CodeDom.Compiler.GeneratedCode("protoc", null)]
    public global::PLUME.Sample.Unity.SceneIdentifier ActiveSceneId {
      get { return activeSceneId_; }
      set {
        activeSceneId_ = value;
      }
    }

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    [global::System.CodeDom.Compiler.GeneratedCode("protoc", null)]
    public override bool Equals(object other) {
      return Equals(other as ChangeActiveScene);
    }

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    [global::System.CodeDom.Compiler.GeneratedCode("protoc", null)]
    public bool Equals(ChangeActiveScene other) {
      if (ReferenceEquals(other, null)) {
        return false;
      }
      if (ReferenceEquals(other, this)) {
        return true;
      }
      if (!object.Equals(ActiveSceneId, other.ActiveSceneId)) return false;
      return Equals(_unknownFields, other._unknownFields);
    }

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    [global::System.CodeDom.Compiler.GeneratedCode("protoc", null)]
    public override int GetHashCode() {
      int hash = 1;
      if (activeSceneId_ != null) hash ^= ActiveSceneId.GetHashCode();
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
      if (activeSceneId_ != null) {
        output.WriteRawTag(10);
        output.WriteMessage(ActiveSceneId);
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
      if (activeSceneId_ != null) {
        output.WriteRawTag(10);
        output.WriteMessage(ActiveSceneId);
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
      if (activeSceneId_ != null) {
        size += 1 + pb::CodedOutputStream.ComputeMessageSize(ActiveSceneId);
      }
      if (_unknownFields != null) {
        size += _unknownFields.CalculateSize();
      }
      return size;
    }

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    [global::System.CodeDom.Compiler.GeneratedCode("protoc", null)]
    public void MergeFrom(ChangeActiveScene other) {
      if (other == null) {
        return;
      }
      if (other.activeSceneId_ != null) {
        if (activeSceneId_ == null) {
          ActiveSceneId = new global::PLUME.Sample.Unity.SceneIdentifier();
        }
        ActiveSceneId.MergeFrom(other.ActiveSceneId);
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
            if (activeSceneId_ == null) {
              ActiveSceneId = new global::PLUME.Sample.Unity.SceneIdentifier();
            }
            input.ReadMessage(ActiveSceneId);
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
            if (activeSceneId_ == null) {
              ActiveSceneId = new global::PLUME.Sample.Unity.SceneIdentifier();
            }
            input.ReadMessage(ActiveSceneId);
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