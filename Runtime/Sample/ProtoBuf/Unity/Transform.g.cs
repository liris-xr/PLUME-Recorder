// <auto-generated>
//     Generated by the protocol buffer compiler.  DO NOT EDIT!
//     source: unity/transform.proto
// </auto-generated>
#pragma warning disable 1591, 0612, 3021, 8981
#region Designer generated code

using pb = global::Google.Protobuf;
using pbc = global::Google.Protobuf.Collections;
using pbr = global::Google.Protobuf.Reflection;
using scg = global::System.Collections.Generic;
namespace PLUME.Sample.Unity {

  /// <summary>Holder for reflection information generated from unity/transform.proto</summary>
  public static partial class TransformReflection {

    #region Descriptor
    /// <summary>File descriptor for unity/transform.proto</summary>
    public static pbr::FileDescriptor Descriptor {
      get { return descriptor; }
    }
    private static pbr::FileDescriptor descriptor;

    static TransformReflection() {
      byte[] descriptorData = global::System.Convert.FromBase64String(
          string.Concat(
            "ChV1bml0eS90cmFuc2Zvcm0ucHJvdG8SEnBsdW1lLnNhbXBsZS51bml0eRoU",
            "Y29tbW9uL3ZlY3RvcjMucHJvdG8aF2NvbW1vbi9xdWF0ZXJuaW9uLnByb3Rv",
            "Ghd1bml0eS9pZGVudGlmaWVycy5wcm90byJGCg9UcmFuc2Zvcm1DcmVhdGUS",
            "MwoCaWQYASABKAsyJy5wbHVtZS5zYW1wbGUudW5pdHkuQ29tcG9uZW50SWRl",
            "bnRpZmllciJHChBUcmFuc2Zvcm1EZXN0cm95EjMKAmlkGAEgASgLMicucGx1",
            "bWUuc2FtcGxlLnVuaXR5LkNvbXBvbmVudElkZW50aWZpZXIipgMKD1RyYW5z",
            "Zm9ybVVwZGF0ZRIzCgJpZBgBIAEoCzInLnBsdW1lLnNhbXBsZS51bml0eS5D",
            "b21wb25lbnRJZGVudGlmaWVyEj8KCXBhcmVudF9pZBgCIAEoCzInLnBsdW1l",
            "LnNhbXBsZS51bml0eS5Db21wb25lbnRJZGVudGlmaWVySACIAQESGAoLc2li",
            "bGluZ19pZHgYAyABKAVIAYgBARI5Cg5sb2NhbF9wb3NpdGlvbhgEIAEoCzIc",
            "LnBsdW1lLnNhbXBsZS5jb21tb24uVmVjdG9yM0gCiAEBEjwKDmxvY2FsX3Jv",
            "dGF0aW9uGAUgASgLMh8ucGx1bWUuc2FtcGxlLmNvbW1vbi5RdWF0ZXJuaW9u",
            "SAOIAQESNgoLbG9jYWxfc2NhbGUYBiABKAsyHC5wbHVtZS5zYW1wbGUuY29t",
            "bW9uLlZlY3RvcjNIBIgBAUIMCgpfcGFyZW50X2lkQg4KDF9zaWJsaW5nX2lk",
            "eEIRCg9fbG9jYWxfcG9zaXRpb25CEQoPX2xvY2FsX3JvdGF0aW9uQg4KDF9s",
            "b2NhbF9zY2FsZUIVqgISUExVTUUuU2FtcGxlLlVuaXR5YgZwcm90bzM="));
      descriptor = pbr::FileDescriptor.FromGeneratedCode(descriptorData,
          new pbr::FileDescriptor[] { global::PLUME.Sample.Common.Vector3Reflection.Descriptor, global::PLUME.Sample.Common.QuaternionReflection.Descriptor, global::PLUME.Sample.Unity.IdentifiersReflection.Descriptor, },
          new pbr::GeneratedClrTypeInfo(null, null, new pbr::GeneratedClrTypeInfo[] {
            new pbr::GeneratedClrTypeInfo(typeof(global::PLUME.Sample.Unity.TransformCreate), global::PLUME.Sample.Unity.TransformCreate.Parser, new[]{ "Id" }, null, null, null, null),
            new pbr::GeneratedClrTypeInfo(typeof(global::PLUME.Sample.Unity.TransformDestroy), global::PLUME.Sample.Unity.TransformDestroy.Parser, new[]{ "Id" }, null, null, null, null),
            new pbr::GeneratedClrTypeInfo(typeof(global::PLUME.Sample.Unity.TransformUpdate), global::PLUME.Sample.Unity.TransformUpdate.Parser, new[]{ "Id", "ParentId", "SiblingIdx", "LocalPosition", "LocalRotation", "LocalScale" }, new[]{ "ParentId", "SiblingIdx", "LocalPosition", "LocalRotation", "LocalScale" }, null, null, null)
          }));
    }
    #endregion

  }
  #region Messages
  [global::System.Diagnostics.DebuggerDisplayAttribute("{ToString(),nq}")]
  public sealed partial class TransformCreate : pb::IMessage<TransformCreate>
  #if !GOOGLE_PROTOBUF_REFSTRUCT_COMPATIBILITY_MODE
      , pb::IBufferMessage
  #endif
  {
    private static readonly pb::MessageParser<TransformCreate> _parser = new pb::MessageParser<TransformCreate>(() => new TransformCreate());
    private pb::UnknownFieldSet _unknownFields;
    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    [global::System.CodeDom.Compiler.GeneratedCode("protoc", null)]
    public static pb::MessageParser<TransformCreate> Parser { get { return _parser; } }

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    [global::System.CodeDom.Compiler.GeneratedCode("protoc", null)]
    public static pbr::MessageDescriptor Descriptor {
      get { return global::PLUME.Sample.Unity.TransformReflection.Descriptor.MessageTypes[0]; }
    }

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    [global::System.CodeDom.Compiler.GeneratedCode("protoc", null)]
    pbr::MessageDescriptor pb::IMessage.Descriptor {
      get { return Descriptor; }
    }

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    [global::System.CodeDom.Compiler.GeneratedCode("protoc", null)]
    public TransformCreate() {
      OnConstruction();
    }

    partial void OnConstruction();

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    [global::System.CodeDom.Compiler.GeneratedCode("protoc", null)]
    public TransformCreate(TransformCreate other) : this() {
      id_ = other.id_ != null ? other.id_.Clone() : null;
      _unknownFields = pb::UnknownFieldSet.Clone(other._unknownFields);
    }

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    [global::System.CodeDom.Compiler.GeneratedCode("protoc", null)]
    public TransformCreate Clone() {
      return new TransformCreate(this);
    }

    /// <summary>Field number for the "id" field.</summary>
    public const int IdFieldNumber = 1;
    private global::PLUME.Sample.Unity.ComponentIdentifier id_;
    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    [global::System.CodeDom.Compiler.GeneratedCode("protoc", null)]
    public global::PLUME.Sample.Unity.ComponentIdentifier Id {
      get { return id_; }
      set {
        id_ = value;
      }
    }

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    [global::System.CodeDom.Compiler.GeneratedCode("protoc", null)]
    public override bool Equals(object other) {
      return Equals(other as TransformCreate);
    }

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    [global::System.CodeDom.Compiler.GeneratedCode("protoc", null)]
    public bool Equals(TransformCreate other) {
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
    public void MergeFrom(TransformCreate other) {
      if (other == null) {
        return;
      }
      if (other.id_ != null) {
        if (id_ == null) {
          Id = new global::PLUME.Sample.Unity.ComponentIdentifier();
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
              Id = new global::PLUME.Sample.Unity.ComponentIdentifier();
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
              Id = new global::PLUME.Sample.Unity.ComponentIdentifier();
            }
            input.ReadMessage(Id);
            break;
          }
        }
      }
    }
    #endif

  }

  [global::System.Diagnostics.DebuggerDisplayAttribute("{ToString(),nq}")]
  public sealed partial class TransformDestroy : pb::IMessage<TransformDestroy>
  #if !GOOGLE_PROTOBUF_REFSTRUCT_COMPATIBILITY_MODE
      , pb::IBufferMessage
  #endif
  {
    private static readonly pb::MessageParser<TransformDestroy> _parser = new pb::MessageParser<TransformDestroy>(() => new TransformDestroy());
    private pb::UnknownFieldSet _unknownFields;
    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    [global::System.CodeDom.Compiler.GeneratedCode("protoc", null)]
    public static pb::MessageParser<TransformDestroy> Parser { get { return _parser; } }

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    [global::System.CodeDom.Compiler.GeneratedCode("protoc", null)]
    public static pbr::MessageDescriptor Descriptor {
      get { return global::PLUME.Sample.Unity.TransformReflection.Descriptor.MessageTypes[1]; }
    }

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    [global::System.CodeDom.Compiler.GeneratedCode("protoc", null)]
    pbr::MessageDescriptor pb::IMessage.Descriptor {
      get { return Descriptor; }
    }

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    [global::System.CodeDom.Compiler.GeneratedCode("protoc", null)]
    public TransformDestroy() {
      OnConstruction();
    }

    partial void OnConstruction();

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    [global::System.CodeDom.Compiler.GeneratedCode("protoc", null)]
    public TransformDestroy(TransformDestroy other) : this() {
      id_ = other.id_ != null ? other.id_.Clone() : null;
      _unknownFields = pb::UnknownFieldSet.Clone(other._unknownFields);
    }

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    [global::System.CodeDom.Compiler.GeneratedCode("protoc", null)]
    public TransformDestroy Clone() {
      return new TransformDestroy(this);
    }

    /// <summary>Field number for the "id" field.</summary>
    public const int IdFieldNumber = 1;
    private global::PLUME.Sample.Unity.ComponentIdentifier id_;
    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    [global::System.CodeDom.Compiler.GeneratedCode("protoc", null)]
    public global::PLUME.Sample.Unity.ComponentIdentifier Id {
      get { return id_; }
      set {
        id_ = value;
      }
    }

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    [global::System.CodeDom.Compiler.GeneratedCode("protoc", null)]
    public override bool Equals(object other) {
      return Equals(other as TransformDestroy);
    }

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    [global::System.CodeDom.Compiler.GeneratedCode("protoc", null)]
    public bool Equals(TransformDestroy other) {
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
    public void MergeFrom(TransformDestroy other) {
      if (other == null) {
        return;
      }
      if (other.id_ != null) {
        if (id_ == null) {
          Id = new global::PLUME.Sample.Unity.ComponentIdentifier();
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
              Id = new global::PLUME.Sample.Unity.ComponentIdentifier();
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
              Id = new global::PLUME.Sample.Unity.ComponentIdentifier();
            }
            input.ReadMessage(Id);
            break;
          }
        }
      }
    }
    #endif

  }

  [global::System.Diagnostics.DebuggerDisplayAttribute("{ToString(),nq}")]
  public sealed partial class TransformUpdate : pb::IMessage<TransformUpdate>
  #if !GOOGLE_PROTOBUF_REFSTRUCT_COMPATIBILITY_MODE
      , pb::IBufferMessage
  #endif
  {
    private static readonly pb::MessageParser<TransformUpdate> _parser = new pb::MessageParser<TransformUpdate>(() => new TransformUpdate());
    private pb::UnknownFieldSet _unknownFields;
    private int _hasBits0;
    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    [global::System.CodeDom.Compiler.GeneratedCode("protoc", null)]
    public static pb::MessageParser<TransformUpdate> Parser { get { return _parser; } }

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    [global::System.CodeDom.Compiler.GeneratedCode("protoc", null)]
    public static pbr::MessageDescriptor Descriptor {
      get { return global::PLUME.Sample.Unity.TransformReflection.Descriptor.MessageTypes[2]; }
    }

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    [global::System.CodeDom.Compiler.GeneratedCode("protoc", null)]
    pbr::MessageDescriptor pb::IMessage.Descriptor {
      get { return Descriptor; }
    }

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    [global::System.CodeDom.Compiler.GeneratedCode("protoc", null)]
    public TransformUpdate() {
      OnConstruction();
    }

    partial void OnConstruction();

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    [global::System.CodeDom.Compiler.GeneratedCode("protoc", null)]
    public TransformUpdate(TransformUpdate other) : this() {
      _hasBits0 = other._hasBits0;
      id_ = other.id_ != null ? other.id_.Clone() : null;
      parentId_ = other.parentId_ != null ? other.parentId_.Clone() : null;
      siblingIdx_ = other.siblingIdx_;
      localPosition_ = other.localPosition_ != null ? other.localPosition_.Clone() : null;
      localRotation_ = other.localRotation_ != null ? other.localRotation_.Clone() : null;
      localScale_ = other.localScale_ != null ? other.localScale_.Clone() : null;
      _unknownFields = pb::UnknownFieldSet.Clone(other._unknownFields);
    }

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    [global::System.CodeDom.Compiler.GeneratedCode("protoc", null)]
    public TransformUpdate Clone() {
      return new TransformUpdate(this);
    }

    /// <summary>Field number for the "id" field.</summary>
    public const int IdFieldNumber = 1;
    private global::PLUME.Sample.Unity.ComponentIdentifier id_;
    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    [global::System.CodeDom.Compiler.GeneratedCode("protoc", null)]
    public global::PLUME.Sample.Unity.ComponentIdentifier Id {
      get { return id_; }
      set {
        id_ = value;
      }
    }

    /// <summary>Field number for the "parent_id" field.</summary>
    public const int ParentIdFieldNumber = 2;
    private global::PLUME.Sample.Unity.ComponentIdentifier parentId_;
    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    [global::System.CodeDom.Compiler.GeneratedCode("protoc", null)]
    public global::PLUME.Sample.Unity.ComponentIdentifier ParentId {
      get { return parentId_; }
      set {
        parentId_ = value;
      }
    }

    /// <summary>Field number for the "sibling_idx" field.</summary>
    public const int SiblingIdxFieldNumber = 3;
    private readonly static int SiblingIdxDefaultValue = 0;

    private int siblingIdx_;
    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    [global::System.CodeDom.Compiler.GeneratedCode("protoc", null)]
    public int SiblingIdx {
      get { if ((_hasBits0 & 1) != 0) { return siblingIdx_; } else { return SiblingIdxDefaultValue; } }
      set {
        _hasBits0 |= 1;
        siblingIdx_ = value;
      }
    }
    /// <summary>Gets whether the "sibling_idx" field is set</summary>
    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    [global::System.CodeDom.Compiler.GeneratedCode("protoc", null)]
    public bool HasSiblingIdx {
      get { return (_hasBits0 & 1) != 0; }
    }
    /// <summary>Clears the value of the "sibling_idx" field</summary>
    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    [global::System.CodeDom.Compiler.GeneratedCode("protoc", null)]
    public void ClearSiblingIdx() {
      _hasBits0 &= ~1;
    }

    /// <summary>Field number for the "local_position" field.</summary>
    public const int LocalPositionFieldNumber = 4;
    private global::PLUME.Sample.Common.Vector3 localPosition_;
    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    [global::System.CodeDom.Compiler.GeneratedCode("protoc", null)]
    public global::PLUME.Sample.Common.Vector3 LocalPosition {
      get { return localPosition_; }
      set {
        localPosition_ = value;
      }
    }

    /// <summary>Field number for the "local_rotation" field.</summary>
    public const int LocalRotationFieldNumber = 5;
    private global::PLUME.Sample.Common.Quaternion localRotation_;
    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    [global::System.CodeDom.Compiler.GeneratedCode("protoc", null)]
    public global::PLUME.Sample.Common.Quaternion LocalRotation {
      get { return localRotation_; }
      set {
        localRotation_ = value;
      }
    }

    /// <summary>Field number for the "local_scale" field.</summary>
    public const int LocalScaleFieldNumber = 6;
    private global::PLUME.Sample.Common.Vector3 localScale_;
    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    [global::System.CodeDom.Compiler.GeneratedCode("protoc", null)]
    public global::PLUME.Sample.Common.Vector3 LocalScale {
      get { return localScale_; }
      set {
        localScale_ = value;
      }
    }

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    [global::System.CodeDom.Compiler.GeneratedCode("protoc", null)]
    public override bool Equals(object other) {
      return Equals(other as TransformUpdate);
    }

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    [global::System.CodeDom.Compiler.GeneratedCode("protoc", null)]
    public bool Equals(TransformUpdate other) {
      if (ReferenceEquals(other, null)) {
        return false;
      }
      if (ReferenceEquals(other, this)) {
        return true;
      }
      if (!object.Equals(Id, other.Id)) return false;
      if (!object.Equals(ParentId, other.ParentId)) return false;
      if (SiblingIdx != other.SiblingIdx) return false;
      if (!object.Equals(LocalPosition, other.LocalPosition)) return false;
      if (!object.Equals(LocalRotation, other.LocalRotation)) return false;
      if (!object.Equals(LocalScale, other.LocalScale)) return false;
      return Equals(_unknownFields, other._unknownFields);
    }

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    [global::System.CodeDom.Compiler.GeneratedCode("protoc", null)]
    public override int GetHashCode() {
      int hash = 1;
      if (id_ != null) hash ^= Id.GetHashCode();
      if (parentId_ != null) hash ^= ParentId.GetHashCode();
      if (HasSiblingIdx) hash ^= SiblingIdx.GetHashCode();
      if (localPosition_ != null) hash ^= LocalPosition.GetHashCode();
      if (localRotation_ != null) hash ^= LocalRotation.GetHashCode();
      if (localScale_ != null) hash ^= LocalScale.GetHashCode();
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
      if (parentId_ != null) {
        output.WriteRawTag(18);
        output.WriteMessage(ParentId);
      }
      if (HasSiblingIdx) {
        output.WriteRawTag(24);
        output.WriteInt32(SiblingIdx);
      }
      if (localPosition_ != null) {
        output.WriteRawTag(34);
        output.WriteMessage(LocalPosition);
      }
      if (localRotation_ != null) {
        output.WriteRawTag(42);
        output.WriteMessage(LocalRotation);
      }
      if (localScale_ != null) {
        output.WriteRawTag(50);
        output.WriteMessage(LocalScale);
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
      if (parentId_ != null) {
        output.WriteRawTag(18);
        output.WriteMessage(ParentId);
      }
      if (HasSiblingIdx) {
        output.WriteRawTag(24);
        output.WriteInt32(SiblingIdx);
      }
      if (localPosition_ != null) {
        output.WriteRawTag(34);
        output.WriteMessage(LocalPosition);
      }
      if (localRotation_ != null) {
        output.WriteRawTag(42);
        output.WriteMessage(LocalRotation);
      }
      if (localScale_ != null) {
        output.WriteRawTag(50);
        output.WriteMessage(LocalScale);
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
      if (parentId_ != null) {
        size += 1 + pb::CodedOutputStream.ComputeMessageSize(ParentId);
      }
      if (HasSiblingIdx) {
        size += 1 + pb::CodedOutputStream.ComputeInt32Size(SiblingIdx);
      }
      if (localPosition_ != null) {
        size += 1 + pb::CodedOutputStream.ComputeMessageSize(LocalPosition);
      }
      if (localRotation_ != null) {
        size += 1 + pb::CodedOutputStream.ComputeMessageSize(LocalRotation);
      }
      if (localScale_ != null) {
        size += 1 + pb::CodedOutputStream.ComputeMessageSize(LocalScale);
      }
      if (_unknownFields != null) {
        size += _unknownFields.CalculateSize();
      }
      return size;
    }

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    [global::System.CodeDom.Compiler.GeneratedCode("protoc", null)]
    public void MergeFrom(TransformUpdate other) {
      if (other == null) {
        return;
      }
      if (other.id_ != null) {
        if (id_ == null) {
          Id = new global::PLUME.Sample.Unity.ComponentIdentifier();
        }
        Id.MergeFrom(other.Id);
      }
      if (other.parentId_ != null) {
        if (parentId_ == null) {
          ParentId = new global::PLUME.Sample.Unity.ComponentIdentifier();
        }
        ParentId.MergeFrom(other.ParentId);
      }
      if (other.HasSiblingIdx) {
        SiblingIdx = other.SiblingIdx;
      }
      if (other.localPosition_ != null) {
        if (localPosition_ == null) {
          LocalPosition = new global::PLUME.Sample.Common.Vector3();
        }
        LocalPosition.MergeFrom(other.LocalPosition);
      }
      if (other.localRotation_ != null) {
        if (localRotation_ == null) {
          LocalRotation = new global::PLUME.Sample.Common.Quaternion();
        }
        LocalRotation.MergeFrom(other.LocalRotation);
      }
      if (other.localScale_ != null) {
        if (localScale_ == null) {
          LocalScale = new global::PLUME.Sample.Common.Vector3();
        }
        LocalScale.MergeFrom(other.LocalScale);
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
              Id = new global::PLUME.Sample.Unity.ComponentIdentifier();
            }
            input.ReadMessage(Id);
            break;
          }
          case 18: {
            if (parentId_ == null) {
              ParentId = new global::PLUME.Sample.Unity.ComponentIdentifier();
            }
            input.ReadMessage(ParentId);
            break;
          }
          case 24: {
            SiblingIdx = input.ReadInt32();
            break;
          }
          case 34: {
            if (localPosition_ == null) {
              LocalPosition = new global::PLUME.Sample.Common.Vector3();
            }
            input.ReadMessage(LocalPosition);
            break;
          }
          case 42: {
            if (localRotation_ == null) {
              LocalRotation = new global::PLUME.Sample.Common.Quaternion();
            }
            input.ReadMessage(LocalRotation);
            break;
          }
          case 50: {
            if (localScale_ == null) {
              LocalScale = new global::PLUME.Sample.Common.Vector3();
            }
            input.ReadMessage(LocalScale);
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
              Id = new global::PLUME.Sample.Unity.ComponentIdentifier();
            }
            input.ReadMessage(Id);
            break;
          }
          case 18: {
            if (parentId_ == null) {
              ParentId = new global::PLUME.Sample.Unity.ComponentIdentifier();
            }
            input.ReadMessage(ParentId);
            break;
          }
          case 24: {
            SiblingIdx = input.ReadInt32();
            break;
          }
          case 34: {
            if (localPosition_ == null) {
              LocalPosition = new global::PLUME.Sample.Common.Vector3();
            }
            input.ReadMessage(LocalPosition);
            break;
          }
          case 42: {
            if (localRotation_ == null) {
              LocalRotation = new global::PLUME.Sample.Common.Quaternion();
            }
            input.ReadMessage(LocalRotation);
            break;
          }
          case 50: {
            if (localScale_ == null) {
              LocalScale = new global::PLUME.Sample.Common.Vector3();
            }
            input.ReadMessage(LocalScale);
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
