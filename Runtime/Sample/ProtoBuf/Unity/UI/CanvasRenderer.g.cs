// <auto-generated>
//     Generated by the protocol buffer compiler.  DO NOT EDIT!
//     source: plume/sample/unity/ui/canvas_renderer.proto
// </auto-generated>
#pragma warning disable 1591, 0612, 3021, 8981
#region Designer generated code

using pb = global::Google.Protobuf;
using pbc = global::Google.Protobuf.Collections;
using pbr = global::Google.Protobuf.Reflection;
using scg = global::System.Collections.Generic;
namespace PLUME.Sample.Unity.UI {

  /// <summary>Holder for reflection information generated from plume/sample/unity/ui/canvas_renderer.proto</summary>
  public static partial class CanvasRendererReflection {

    #region Descriptor
    /// <summary>File descriptor for plume/sample/unity/ui/canvas_renderer.proto</summary>
    public static pbr::FileDescriptor Descriptor {
      get { return descriptor; }
    }
    private static pbr::FileDescriptor descriptor;

    static CanvasRendererReflection() {
      byte[] descriptorData = global::System.Convert.FromBase64String(
          string.Concat(
            "CitwbHVtZS9zYW1wbGUvdW5pdHkvdWkvY2FudmFzX3JlbmRlcmVyLnByb3Rv",
            "EhVwbHVtZS5zYW1wbGUudW5pdHkudWkaJHBsdW1lL3NhbXBsZS91bml0eS9p",
            "ZGVudGlmaWVycy5wcm90byJdChRDYW52YXNSZW5kZXJlckNyZWF0ZRJFCglj",
            "b21wb25lbnQYASABKAsyJy5wbHVtZS5zYW1wbGUudW5pdHkuQ29tcG9uZW50",
            "SWRlbnRpZmllclIJY29tcG9uZW50Il4KFUNhbnZhc1JlbmRlcmVyRGVzdHJv",
            "eRJFCgljb21wb25lbnQYASABKAsyJy5wbHVtZS5zYW1wbGUudW5pdHkuQ29t",
            "cG9uZW50SWRlbnRpZmllclIJY29tcG9uZW50IrABChRDYW52YXNSZW5kZXJl",
            "clVwZGF0ZRJFCgljb21wb25lbnQYASABKAsyJy5wbHVtZS5zYW1wbGUudW5p",
            "dHkuQ29tcG9uZW50SWRlbnRpZmllclIJY29tcG9uZW50EjcKFWN1bGxfdHJh",
            "bnNwYXJlbnRfbWVzaBgCIAEoCEgAUhNjdWxsVHJhbnNwYXJlbnRNZXNoiAEB",
            "QhgKFl9jdWxsX3RyYW5zcGFyZW50X21lc2hCGKoCFVBMVU1FLlNhbXBsZS5V",
            "bml0eS5VSWIGcHJvdG8z"));
      descriptor = pbr::FileDescriptor.FromGeneratedCode(descriptorData,
          new pbr::FileDescriptor[] { global::PLUME.Sample.Unity.IdentifiersReflection.Descriptor, },
          new pbr::GeneratedClrTypeInfo(null, null, new pbr::GeneratedClrTypeInfo[] {
            new pbr::GeneratedClrTypeInfo(typeof(global::PLUME.Sample.Unity.UI.CanvasRendererCreate), global::PLUME.Sample.Unity.UI.CanvasRendererCreate.Parser, new[]{ "Component" }, null, null, null, null),
            new pbr::GeneratedClrTypeInfo(typeof(global::PLUME.Sample.Unity.UI.CanvasRendererDestroy), global::PLUME.Sample.Unity.UI.CanvasRendererDestroy.Parser, new[]{ "Component" }, null, null, null, null),
            new pbr::GeneratedClrTypeInfo(typeof(global::PLUME.Sample.Unity.UI.CanvasRendererUpdate), global::PLUME.Sample.Unity.UI.CanvasRendererUpdate.Parser, new[]{ "Component", "CullTransparentMesh" }, new[]{ "CullTransparentMesh" }, null, null, null)
          }));
    }
    #endregion

  }
  #region Messages
  [global::System.Diagnostics.DebuggerDisplayAttribute("{ToString(),nq}")]
  public sealed partial class CanvasRendererCreate : pb::IMessage<CanvasRendererCreate>
  #if !GOOGLE_PROTOBUF_REFSTRUCT_COMPATIBILITY_MODE
      , pb::IBufferMessage
  #endif
  {
    private static readonly pb::MessageParser<CanvasRendererCreate> _parser = new pb::MessageParser<CanvasRendererCreate>(() => new CanvasRendererCreate());
    private pb::UnknownFieldSet _unknownFields;
    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    [global::System.CodeDom.Compiler.GeneratedCode("protoc", null)]
    public static pb::MessageParser<CanvasRendererCreate> Parser { get { return _parser; } }

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    [global::System.CodeDom.Compiler.GeneratedCode("protoc", null)]
    public static pbr::MessageDescriptor Descriptor {
      get { return global::PLUME.Sample.Unity.UI.CanvasRendererReflection.Descriptor.MessageTypes[0]; }
    }

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    [global::System.CodeDom.Compiler.GeneratedCode("protoc", null)]
    pbr::MessageDescriptor pb::IMessage.Descriptor {
      get { return Descriptor; }
    }

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    [global::System.CodeDom.Compiler.GeneratedCode("protoc", null)]
    public CanvasRendererCreate() {
      OnConstruction();
    }

    partial void OnConstruction();

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    [global::System.CodeDom.Compiler.GeneratedCode("protoc", null)]
    public CanvasRendererCreate(CanvasRendererCreate other) : this() {
      component_ = other.component_ != null ? other.component_.Clone() : null;
      _unknownFields = pb::UnknownFieldSet.Clone(other._unknownFields);
    }

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    [global::System.CodeDom.Compiler.GeneratedCode("protoc", null)]
    public CanvasRendererCreate Clone() {
      return new CanvasRendererCreate(this);
    }

    /// <summary>Field number for the "component" field.</summary>
    public const int ComponentFieldNumber = 1;
    private global::PLUME.Sample.Unity.ComponentIdentifier component_;
    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    [global::System.CodeDom.Compiler.GeneratedCode("protoc", null)]
    public global::PLUME.Sample.Unity.ComponentIdentifier Component {
      get { return component_; }
      set {
        component_ = value;
      }
    }

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    [global::System.CodeDom.Compiler.GeneratedCode("protoc", null)]
    public override bool Equals(object other) {
      return Equals(other as CanvasRendererCreate);
    }

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    [global::System.CodeDom.Compiler.GeneratedCode("protoc", null)]
    public bool Equals(CanvasRendererCreate other) {
      if (ReferenceEquals(other, null)) {
        return false;
      }
      if (ReferenceEquals(other, this)) {
        return true;
      }
      if (!object.Equals(Component, other.Component)) return false;
      return Equals(_unknownFields, other._unknownFields);
    }

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    [global::System.CodeDom.Compiler.GeneratedCode("protoc", null)]
    public override int GetHashCode() {
      int hash = 1;
      if (component_ != null) hash ^= Component.GetHashCode();
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
      if (component_ != null) {
        output.WriteRawTag(10);
        output.WriteMessage(Component);
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
      if (component_ != null) {
        output.WriteRawTag(10);
        output.WriteMessage(Component);
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
      if (component_ != null) {
        size += 1 + pb::CodedOutputStream.ComputeMessageSize(Component);
      }
      if (_unknownFields != null) {
        size += _unknownFields.CalculateSize();
      }
      return size;
    }

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    [global::System.CodeDom.Compiler.GeneratedCode("protoc", null)]
    public void MergeFrom(CanvasRendererCreate other) {
      if (other == null) {
        return;
      }
      if (other.component_ != null) {
        if (component_ == null) {
          Component = new global::PLUME.Sample.Unity.ComponentIdentifier();
        }
        Component.MergeFrom(other.Component);
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
      if ((tag & 7) == 4) {
        // Abort on any end group tag.
        return;
      }
      switch(tag) {
          default:
            _unknownFields = pb::UnknownFieldSet.MergeFieldFrom(_unknownFields, input);
            break;
          case 10: {
            if (component_ == null) {
              Component = new global::PLUME.Sample.Unity.ComponentIdentifier();
            }
            input.ReadMessage(Component);
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
      if ((tag & 7) == 4) {
        // Abort on any end group tag.
        return;
      }
      switch(tag) {
          default:
            _unknownFields = pb::UnknownFieldSet.MergeFieldFrom(_unknownFields, ref input);
            break;
          case 10: {
            if (component_ == null) {
              Component = new global::PLUME.Sample.Unity.ComponentIdentifier();
            }
            input.ReadMessage(Component);
            break;
          }
        }
      }
    }
    #endif

  }

  [global::System.Diagnostics.DebuggerDisplayAttribute("{ToString(),nq}")]
  public sealed partial class CanvasRendererDestroy : pb::IMessage<CanvasRendererDestroy>
  #if !GOOGLE_PROTOBUF_REFSTRUCT_COMPATIBILITY_MODE
      , pb::IBufferMessage
  #endif
  {
    private static readonly pb::MessageParser<CanvasRendererDestroy> _parser = new pb::MessageParser<CanvasRendererDestroy>(() => new CanvasRendererDestroy());
    private pb::UnknownFieldSet _unknownFields;
    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    [global::System.CodeDom.Compiler.GeneratedCode("protoc", null)]
    public static pb::MessageParser<CanvasRendererDestroy> Parser { get { return _parser; } }

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    [global::System.CodeDom.Compiler.GeneratedCode("protoc", null)]
    public static pbr::MessageDescriptor Descriptor {
      get { return global::PLUME.Sample.Unity.UI.CanvasRendererReflection.Descriptor.MessageTypes[1]; }
    }

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    [global::System.CodeDom.Compiler.GeneratedCode("protoc", null)]
    pbr::MessageDescriptor pb::IMessage.Descriptor {
      get { return Descriptor; }
    }

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    [global::System.CodeDom.Compiler.GeneratedCode("protoc", null)]
    public CanvasRendererDestroy() {
      OnConstruction();
    }

    partial void OnConstruction();

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    [global::System.CodeDom.Compiler.GeneratedCode("protoc", null)]
    public CanvasRendererDestroy(CanvasRendererDestroy other) : this() {
      component_ = other.component_ != null ? other.component_.Clone() : null;
      _unknownFields = pb::UnknownFieldSet.Clone(other._unknownFields);
    }

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    [global::System.CodeDom.Compiler.GeneratedCode("protoc", null)]
    public CanvasRendererDestroy Clone() {
      return new CanvasRendererDestroy(this);
    }

    /// <summary>Field number for the "component" field.</summary>
    public const int ComponentFieldNumber = 1;
    private global::PLUME.Sample.Unity.ComponentIdentifier component_;
    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    [global::System.CodeDom.Compiler.GeneratedCode("protoc", null)]
    public global::PLUME.Sample.Unity.ComponentIdentifier Component {
      get { return component_; }
      set {
        component_ = value;
      }
    }

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    [global::System.CodeDom.Compiler.GeneratedCode("protoc", null)]
    public override bool Equals(object other) {
      return Equals(other as CanvasRendererDestroy);
    }

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    [global::System.CodeDom.Compiler.GeneratedCode("protoc", null)]
    public bool Equals(CanvasRendererDestroy other) {
      if (ReferenceEquals(other, null)) {
        return false;
      }
      if (ReferenceEquals(other, this)) {
        return true;
      }
      if (!object.Equals(Component, other.Component)) return false;
      return Equals(_unknownFields, other._unknownFields);
    }

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    [global::System.CodeDom.Compiler.GeneratedCode("protoc", null)]
    public override int GetHashCode() {
      int hash = 1;
      if (component_ != null) hash ^= Component.GetHashCode();
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
      if (component_ != null) {
        output.WriteRawTag(10);
        output.WriteMessage(Component);
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
      if (component_ != null) {
        output.WriteRawTag(10);
        output.WriteMessage(Component);
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
      if (component_ != null) {
        size += 1 + pb::CodedOutputStream.ComputeMessageSize(Component);
      }
      if (_unknownFields != null) {
        size += _unknownFields.CalculateSize();
      }
      return size;
    }

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    [global::System.CodeDom.Compiler.GeneratedCode("protoc", null)]
    public void MergeFrom(CanvasRendererDestroy other) {
      if (other == null) {
        return;
      }
      if (other.component_ != null) {
        if (component_ == null) {
          Component = new global::PLUME.Sample.Unity.ComponentIdentifier();
        }
        Component.MergeFrom(other.Component);
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
      if ((tag & 7) == 4) {
        // Abort on any end group tag.
        return;
      }
      switch(tag) {
          default:
            _unknownFields = pb::UnknownFieldSet.MergeFieldFrom(_unknownFields, input);
            break;
          case 10: {
            if (component_ == null) {
              Component = new global::PLUME.Sample.Unity.ComponentIdentifier();
            }
            input.ReadMessage(Component);
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
      if ((tag & 7) == 4) {
        // Abort on any end group tag.
        return;
      }
      switch(tag) {
          default:
            _unknownFields = pb::UnknownFieldSet.MergeFieldFrom(_unknownFields, ref input);
            break;
          case 10: {
            if (component_ == null) {
              Component = new global::PLUME.Sample.Unity.ComponentIdentifier();
            }
            input.ReadMessage(Component);
            break;
          }
        }
      }
    }
    #endif

  }

  [global::System.Diagnostics.DebuggerDisplayAttribute("{ToString(),nq}")]
  public sealed partial class CanvasRendererUpdate : pb::IMessage<CanvasRendererUpdate>
  #if !GOOGLE_PROTOBUF_REFSTRUCT_COMPATIBILITY_MODE
      , pb::IBufferMessage
  #endif
  {
    private static readonly pb::MessageParser<CanvasRendererUpdate> _parser = new pb::MessageParser<CanvasRendererUpdate>(() => new CanvasRendererUpdate());
    private pb::UnknownFieldSet _unknownFields;
    private int _hasBits0;
    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    [global::System.CodeDom.Compiler.GeneratedCode("protoc", null)]
    public static pb::MessageParser<CanvasRendererUpdate> Parser { get { return _parser; } }

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    [global::System.CodeDom.Compiler.GeneratedCode("protoc", null)]
    public static pbr::MessageDescriptor Descriptor {
      get { return global::PLUME.Sample.Unity.UI.CanvasRendererReflection.Descriptor.MessageTypes[2]; }
    }

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    [global::System.CodeDom.Compiler.GeneratedCode("protoc", null)]
    pbr::MessageDescriptor pb::IMessage.Descriptor {
      get { return Descriptor; }
    }

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    [global::System.CodeDom.Compiler.GeneratedCode("protoc", null)]
    public CanvasRendererUpdate() {
      OnConstruction();
    }

    partial void OnConstruction();

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    [global::System.CodeDom.Compiler.GeneratedCode("protoc", null)]
    public CanvasRendererUpdate(CanvasRendererUpdate other) : this() {
      _hasBits0 = other._hasBits0;
      component_ = other.component_ != null ? other.component_.Clone() : null;
      cullTransparentMesh_ = other.cullTransparentMesh_;
      _unknownFields = pb::UnknownFieldSet.Clone(other._unknownFields);
    }

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    [global::System.CodeDom.Compiler.GeneratedCode("protoc", null)]
    public CanvasRendererUpdate Clone() {
      return new CanvasRendererUpdate(this);
    }

    /// <summary>Field number for the "component" field.</summary>
    public const int ComponentFieldNumber = 1;
    private global::PLUME.Sample.Unity.ComponentIdentifier component_;
    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    [global::System.CodeDom.Compiler.GeneratedCode("protoc", null)]
    public global::PLUME.Sample.Unity.ComponentIdentifier Component {
      get { return component_; }
      set {
        component_ = value;
      }
    }

    /// <summary>Field number for the "cull_transparent_mesh" field.</summary>
    public const int CullTransparentMeshFieldNumber = 2;
    private readonly static bool CullTransparentMeshDefaultValue = false;

    private bool cullTransparentMesh_;
    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    [global::System.CodeDom.Compiler.GeneratedCode("protoc", null)]
    public bool CullTransparentMesh {
      get { if ((_hasBits0 & 1) != 0) { return cullTransparentMesh_; } else { return CullTransparentMeshDefaultValue; } }
      set {
        _hasBits0 |= 1;
        cullTransparentMesh_ = value;
      }
    }
    /// <summary>Gets whether the "cull_transparent_mesh" field is set</summary>
    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    [global::System.CodeDom.Compiler.GeneratedCode("protoc", null)]
    public bool HasCullTransparentMesh {
      get { return (_hasBits0 & 1) != 0; }
    }
    /// <summary>Clears the value of the "cull_transparent_mesh" field</summary>
    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    [global::System.CodeDom.Compiler.GeneratedCode("protoc", null)]
    public void ClearCullTransparentMesh() {
      _hasBits0 &= ~1;
    }

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    [global::System.CodeDom.Compiler.GeneratedCode("protoc", null)]
    public override bool Equals(object other) {
      return Equals(other as CanvasRendererUpdate);
    }

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    [global::System.CodeDom.Compiler.GeneratedCode("protoc", null)]
    public bool Equals(CanvasRendererUpdate other) {
      if (ReferenceEquals(other, null)) {
        return false;
      }
      if (ReferenceEquals(other, this)) {
        return true;
      }
      if (!object.Equals(Component, other.Component)) return false;
      if (CullTransparentMesh != other.CullTransparentMesh) return false;
      return Equals(_unknownFields, other._unknownFields);
    }

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    [global::System.CodeDom.Compiler.GeneratedCode("protoc", null)]
    public override int GetHashCode() {
      int hash = 1;
      if (component_ != null) hash ^= Component.GetHashCode();
      if (HasCullTransparentMesh) hash ^= CullTransparentMesh.GetHashCode();
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
      if (component_ != null) {
        output.WriteRawTag(10);
        output.WriteMessage(Component);
      }
      if (HasCullTransparentMesh) {
        output.WriteRawTag(16);
        output.WriteBool(CullTransparentMesh);
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
      if (component_ != null) {
        output.WriteRawTag(10);
        output.WriteMessage(Component);
      }
      if (HasCullTransparentMesh) {
        output.WriteRawTag(16);
        output.WriteBool(CullTransparentMesh);
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
      if (component_ != null) {
        size += 1 + pb::CodedOutputStream.ComputeMessageSize(Component);
      }
      if (HasCullTransparentMesh) {
        size += 1 + 1;
      }
      if (_unknownFields != null) {
        size += _unknownFields.CalculateSize();
      }
      return size;
    }

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    [global::System.CodeDom.Compiler.GeneratedCode("protoc", null)]
    public void MergeFrom(CanvasRendererUpdate other) {
      if (other == null) {
        return;
      }
      if (other.component_ != null) {
        if (component_ == null) {
          Component = new global::PLUME.Sample.Unity.ComponentIdentifier();
        }
        Component.MergeFrom(other.Component);
      }
      if (other.HasCullTransparentMesh) {
        CullTransparentMesh = other.CullTransparentMesh;
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
      if ((tag & 7) == 4) {
        // Abort on any end group tag.
        return;
      }
      switch(tag) {
          default:
            _unknownFields = pb::UnknownFieldSet.MergeFieldFrom(_unknownFields, input);
            break;
          case 10: {
            if (component_ == null) {
              Component = new global::PLUME.Sample.Unity.ComponentIdentifier();
            }
            input.ReadMessage(Component);
            break;
          }
          case 16: {
            CullTransparentMesh = input.ReadBool();
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
      if ((tag & 7) == 4) {
        // Abort on any end group tag.
        return;
      }
      switch(tag) {
          default:
            _unknownFields = pb::UnknownFieldSet.MergeFieldFrom(_unknownFields, ref input);
            break;
          case 10: {
            if (component_ == null) {
              Component = new global::PLUME.Sample.Unity.ComponentIdentifier();
            }
            input.ReadMessage(Component);
            break;
          }
          case 16: {
            CullTransparentMesh = input.ReadBool();
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
