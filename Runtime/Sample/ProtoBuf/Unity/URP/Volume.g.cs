// <auto-generated>
//     Generated by the protocol buffer compiler.  DO NOT EDIT!
//     source: unity/urp/volume.proto
// </auto-generated>
#pragma warning disable 1591, 0612, 3021, 8981
#region Designer generated code

using pb = global::Google.Protobuf;
using pbc = global::Google.Protobuf.Collections;
using pbr = global::Google.Protobuf.Reflection;
using scg = global::System.Collections.Generic;
namespace PLUME.Sample.Unity.URP {

  /// <summary>Holder for reflection information generated from unity/urp/volume.proto</summary>
  public static partial class VolumeReflection {

    #region Descriptor
    /// <summary>File descriptor for unity/urp/volume.proto</summary>
    public static pbr::FileDescriptor Descriptor {
      get { return descriptor; }
    }
    private static pbr::FileDescriptor descriptor;

    static VolumeReflection() {
      byte[] descriptorData = global::System.Convert.FromBase64String(
          string.Concat(
            "ChZ1bml0eS91cnAvdm9sdW1lLnByb3RvEhZwbHVtZS5zYW1wbGUudW5pdHku",
            "dXJwGhd1bml0eS9pZGVudGlmaWVycy5wcm90byJDCgxWb2x1bWVDcmVhdGUS",
            "MwoCaWQYASABKAsyJy5wbHVtZS5zYW1wbGUudW5pdHkuQ29tcG9uZW50SWRl",
            "bnRpZmllciKrAgoMVm9sdW1lVXBkYXRlEjMKAmlkGAEgASgLMicucGx1bWUu",
            "c2FtcGxlLnVuaXR5LkNvbXBvbmVudElkZW50aWZpZXISEQoJaXNfZ2xvYmFs",
            "GAIgASgIEj4KDWNvbGxpZGVyc19pZHMYAyADKAsyJy5wbHVtZS5zYW1wbGUu",
            "dW5pdHkuQ29tcG9uZW50SWRlbnRpZmllchIWCg5ibGVuZF9kaXN0YW5jZRgE",
            "IAEoAhIOCgZ3ZWlnaHQYBSABKAISEAoIcHJpb3JpdHkYBiABKAISQwoRc2hh",
            "cmVkX3Byb2ZpbGVfaWQYByABKAsyIy5wbHVtZS5zYW1wbGUudW5pdHkuQXNz",
            "ZXRJZGVudGlmaWVySACIAQFCFAoSX3NoYXJlZF9wcm9maWxlX2lkIlsKE1Zv",
            "bHVtZVVwZGF0ZUVuYWJsZWQSMwoCaWQYASABKAsyJy5wbHVtZS5zYW1wbGUu",
            "dW5pdHkuQ29tcG9uZW50SWRlbnRpZmllchIPCgdlbmFibGVkGAIgASgIQhmq",
            "AhZQTFVNRS5TYW1wbGUuVW5pdHkuVVJQYgZwcm90bzM="));
      descriptor = pbr::FileDescriptor.FromGeneratedCode(descriptorData,
          new pbr::FileDescriptor[] { global::PLUME.Sample.Unity.IdentifiersReflection.Descriptor, },
          new pbr::GeneratedClrTypeInfo(null, null, new pbr::GeneratedClrTypeInfo[] {
            new pbr::GeneratedClrTypeInfo(typeof(global::PLUME.Sample.Unity.URP.VolumeCreate), global::PLUME.Sample.Unity.URP.VolumeCreate.Parser, new[]{ "Id" }, null, null, null, null),
            new pbr::GeneratedClrTypeInfo(typeof(global::PLUME.Sample.Unity.URP.VolumeUpdate), global::PLUME.Sample.Unity.URP.VolumeUpdate.Parser, new[]{ "Id", "IsGlobal", "CollidersIds", "BlendDistance", "Weight", "Priority", "SharedProfileId" }, new[]{ "SharedProfileId" }, null, null, null),
            new pbr::GeneratedClrTypeInfo(typeof(global::PLUME.Sample.Unity.URP.VolumeUpdateEnabled), global::PLUME.Sample.Unity.URP.VolumeUpdateEnabled.Parser, new[]{ "Id", "Enabled" }, null, null, null, null)
          }));
    }
    #endregion

  }
  #region Messages
  [global::System.Diagnostics.DebuggerDisplayAttribute("{ToString(),nq}")]
  public sealed partial class VolumeCreate : pb::IMessage<VolumeCreate>
  #if !GOOGLE_PROTOBUF_REFSTRUCT_COMPATIBILITY_MODE
      , pb::IBufferMessage
  #endif
  {
    private static readonly pb::MessageParser<VolumeCreate> _parser = new pb::MessageParser<VolumeCreate>(() => new VolumeCreate());
    private pb::UnknownFieldSet _unknownFields;
    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    [global::System.CodeDom.Compiler.GeneratedCode("protoc", null)]
    public static pb::MessageParser<VolumeCreate> Parser { get { return _parser; } }

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    [global::System.CodeDom.Compiler.GeneratedCode("protoc", null)]
    public static pbr::MessageDescriptor Descriptor {
      get { return global::PLUME.Sample.Unity.URP.VolumeReflection.Descriptor.MessageTypes[0]; }
    }

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    [global::System.CodeDom.Compiler.GeneratedCode("protoc", null)]
    pbr::MessageDescriptor pb::IMessage.Descriptor {
      get { return Descriptor; }
    }

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    [global::System.CodeDom.Compiler.GeneratedCode("protoc", null)]
    public VolumeCreate() {
      OnConstruction();
    }

    partial void OnConstruction();

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    [global::System.CodeDom.Compiler.GeneratedCode("protoc", null)]
    public VolumeCreate(VolumeCreate other) : this() {
      id_ = other.id_ != null ? other.id_.Clone() : null;
      _unknownFields = pb::UnknownFieldSet.Clone(other._unknownFields);
    }

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    [global::System.CodeDom.Compiler.GeneratedCode("protoc", null)]
    public VolumeCreate Clone() {
      return new VolumeCreate(this);
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
      return Equals(other as VolumeCreate);
    }

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    [global::System.CodeDom.Compiler.GeneratedCode("protoc", null)]
    public bool Equals(VolumeCreate other) {
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
    public void MergeFrom(VolumeCreate other) {
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
  public sealed partial class VolumeUpdate : pb::IMessage<VolumeUpdate>
  #if !GOOGLE_PROTOBUF_REFSTRUCT_COMPATIBILITY_MODE
      , pb::IBufferMessage
  #endif
  {
    private static readonly pb::MessageParser<VolumeUpdate> _parser = new pb::MessageParser<VolumeUpdate>(() => new VolumeUpdate());
    private pb::UnknownFieldSet _unknownFields;
    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    [global::System.CodeDom.Compiler.GeneratedCode("protoc", null)]
    public static pb::MessageParser<VolumeUpdate> Parser { get { return _parser; } }

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    [global::System.CodeDom.Compiler.GeneratedCode("protoc", null)]
    public static pbr::MessageDescriptor Descriptor {
      get { return global::PLUME.Sample.Unity.URP.VolumeReflection.Descriptor.MessageTypes[1]; }
    }

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    [global::System.CodeDom.Compiler.GeneratedCode("protoc", null)]
    pbr::MessageDescriptor pb::IMessage.Descriptor {
      get { return Descriptor; }
    }

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    [global::System.CodeDom.Compiler.GeneratedCode("protoc", null)]
    public VolumeUpdate() {
      OnConstruction();
    }

    partial void OnConstruction();

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    [global::System.CodeDom.Compiler.GeneratedCode("protoc", null)]
    public VolumeUpdate(VolumeUpdate other) : this() {
      id_ = other.id_ != null ? other.id_.Clone() : null;
      isGlobal_ = other.isGlobal_;
      collidersIds_ = other.collidersIds_.Clone();
      blendDistance_ = other.blendDistance_;
      weight_ = other.weight_;
      priority_ = other.priority_;
      sharedProfileId_ = other.sharedProfileId_ != null ? other.sharedProfileId_.Clone() : null;
      _unknownFields = pb::UnknownFieldSet.Clone(other._unknownFields);
    }

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    [global::System.CodeDom.Compiler.GeneratedCode("protoc", null)]
    public VolumeUpdate Clone() {
      return new VolumeUpdate(this);
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

    /// <summary>Field number for the "is_global" field.</summary>
    public const int IsGlobalFieldNumber = 2;
    private bool isGlobal_;
    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    [global::System.CodeDom.Compiler.GeneratedCode("protoc", null)]
    public bool IsGlobal {
      get { return isGlobal_; }
      set {
        isGlobal_ = value;
      }
    }

    /// <summary>Field number for the "colliders_ids" field.</summary>
    public const int CollidersIdsFieldNumber = 3;
    private static readonly pb::FieldCodec<global::PLUME.Sample.Unity.ComponentIdentifier> _repeated_collidersIds_codec
        = pb::FieldCodec.ForMessage(26, global::PLUME.Sample.Unity.ComponentIdentifier.Parser);
    private readonly pbc::RepeatedField<global::PLUME.Sample.Unity.ComponentIdentifier> collidersIds_ = new pbc::RepeatedField<global::PLUME.Sample.Unity.ComponentIdentifier>();
    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    [global::System.CodeDom.Compiler.GeneratedCode("protoc", null)]
    public pbc::RepeatedField<global::PLUME.Sample.Unity.ComponentIdentifier> CollidersIds {
      get { return collidersIds_; }
    }

    /// <summary>Field number for the "blend_distance" field.</summary>
    public const int BlendDistanceFieldNumber = 4;
    private float blendDistance_;
    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    [global::System.CodeDom.Compiler.GeneratedCode("protoc", null)]
    public float BlendDistance {
      get { return blendDistance_; }
      set {
        blendDistance_ = value;
      }
    }

    /// <summary>Field number for the "weight" field.</summary>
    public const int WeightFieldNumber = 5;
    private float weight_;
    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    [global::System.CodeDom.Compiler.GeneratedCode("protoc", null)]
    public float Weight {
      get { return weight_; }
      set {
        weight_ = value;
      }
    }

    /// <summary>Field number for the "priority" field.</summary>
    public const int PriorityFieldNumber = 6;
    private float priority_;
    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    [global::System.CodeDom.Compiler.GeneratedCode("protoc", null)]
    public float Priority {
      get { return priority_; }
      set {
        priority_ = value;
      }
    }

    /// <summary>Field number for the "shared_profile_id" field.</summary>
    public const int SharedProfileIdFieldNumber = 7;
    private global::PLUME.Sample.Unity.AssetIdentifier sharedProfileId_;
    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    [global::System.CodeDom.Compiler.GeneratedCode("protoc", null)]
    public global::PLUME.Sample.Unity.AssetIdentifier SharedProfileId {
      get { return sharedProfileId_; }
      set {
        sharedProfileId_ = value;
      }
    }

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    [global::System.CodeDom.Compiler.GeneratedCode("protoc", null)]
    public override bool Equals(object other) {
      return Equals(other as VolumeUpdate);
    }

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    [global::System.CodeDom.Compiler.GeneratedCode("protoc", null)]
    public bool Equals(VolumeUpdate other) {
      if (ReferenceEquals(other, null)) {
        return false;
      }
      if (ReferenceEquals(other, this)) {
        return true;
      }
      if (!object.Equals(Id, other.Id)) return false;
      if (IsGlobal != other.IsGlobal) return false;
      if(!collidersIds_.Equals(other.collidersIds_)) return false;
      if (!pbc::ProtobufEqualityComparers.BitwiseSingleEqualityComparer.Equals(BlendDistance, other.BlendDistance)) return false;
      if (!pbc::ProtobufEqualityComparers.BitwiseSingleEqualityComparer.Equals(Weight, other.Weight)) return false;
      if (!pbc::ProtobufEqualityComparers.BitwiseSingleEqualityComparer.Equals(Priority, other.Priority)) return false;
      if (!object.Equals(SharedProfileId, other.SharedProfileId)) return false;
      return Equals(_unknownFields, other._unknownFields);
    }

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    [global::System.CodeDom.Compiler.GeneratedCode("protoc", null)]
    public override int GetHashCode() {
      int hash = 1;
      if (id_ != null) hash ^= Id.GetHashCode();
      if (IsGlobal != false) hash ^= IsGlobal.GetHashCode();
      hash ^= collidersIds_.GetHashCode();
      if (BlendDistance != 0F) hash ^= pbc::ProtobufEqualityComparers.BitwiseSingleEqualityComparer.GetHashCode(BlendDistance);
      if (Weight != 0F) hash ^= pbc::ProtobufEqualityComparers.BitwiseSingleEqualityComparer.GetHashCode(Weight);
      if (Priority != 0F) hash ^= pbc::ProtobufEqualityComparers.BitwiseSingleEqualityComparer.GetHashCode(Priority);
      if (sharedProfileId_ != null) hash ^= SharedProfileId.GetHashCode();
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
      if (IsGlobal != false) {
        output.WriteRawTag(16);
        output.WriteBool(IsGlobal);
      }
      collidersIds_.WriteTo(output, _repeated_collidersIds_codec);
      if (BlendDistance != 0F) {
        output.WriteRawTag(37);
        output.WriteFloat(BlendDistance);
      }
      if (Weight != 0F) {
        output.WriteRawTag(45);
        output.WriteFloat(Weight);
      }
      if (Priority != 0F) {
        output.WriteRawTag(53);
        output.WriteFloat(Priority);
      }
      if (sharedProfileId_ != null) {
        output.WriteRawTag(58);
        output.WriteMessage(SharedProfileId);
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
      if (IsGlobal != false) {
        output.WriteRawTag(16);
        output.WriteBool(IsGlobal);
      }
      collidersIds_.WriteTo(ref output, _repeated_collidersIds_codec);
      if (BlendDistance != 0F) {
        output.WriteRawTag(37);
        output.WriteFloat(BlendDistance);
      }
      if (Weight != 0F) {
        output.WriteRawTag(45);
        output.WriteFloat(Weight);
      }
      if (Priority != 0F) {
        output.WriteRawTag(53);
        output.WriteFloat(Priority);
      }
      if (sharedProfileId_ != null) {
        output.WriteRawTag(58);
        output.WriteMessage(SharedProfileId);
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
      if (IsGlobal != false) {
        size += 1 + 1;
      }
      size += collidersIds_.CalculateSize(_repeated_collidersIds_codec);
      if (BlendDistance != 0F) {
        size += 1 + 4;
      }
      if (Weight != 0F) {
        size += 1 + 4;
      }
      if (Priority != 0F) {
        size += 1 + 4;
      }
      if (sharedProfileId_ != null) {
        size += 1 + pb::CodedOutputStream.ComputeMessageSize(SharedProfileId);
      }
      if (_unknownFields != null) {
        size += _unknownFields.CalculateSize();
      }
      return size;
    }

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    [global::System.CodeDom.Compiler.GeneratedCode("protoc", null)]
    public void MergeFrom(VolumeUpdate other) {
      if (other == null) {
        return;
      }
      if (other.id_ != null) {
        if (id_ == null) {
          Id = new global::PLUME.Sample.Unity.ComponentIdentifier();
        }
        Id.MergeFrom(other.Id);
      }
      if (other.IsGlobal != false) {
        IsGlobal = other.IsGlobal;
      }
      collidersIds_.Add(other.collidersIds_);
      if (other.BlendDistance != 0F) {
        BlendDistance = other.BlendDistance;
      }
      if (other.Weight != 0F) {
        Weight = other.Weight;
      }
      if (other.Priority != 0F) {
        Priority = other.Priority;
      }
      if (other.sharedProfileId_ != null) {
        if (sharedProfileId_ == null) {
          SharedProfileId = new global::PLUME.Sample.Unity.AssetIdentifier();
        }
        SharedProfileId.MergeFrom(other.SharedProfileId);
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
          case 16: {
            IsGlobal = input.ReadBool();
            break;
          }
          case 26: {
            collidersIds_.AddEntriesFrom(input, _repeated_collidersIds_codec);
            break;
          }
          case 37: {
            BlendDistance = input.ReadFloat();
            break;
          }
          case 45: {
            Weight = input.ReadFloat();
            break;
          }
          case 53: {
            Priority = input.ReadFloat();
            break;
          }
          case 58: {
            if (sharedProfileId_ == null) {
              SharedProfileId = new global::PLUME.Sample.Unity.AssetIdentifier();
            }
            input.ReadMessage(SharedProfileId);
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
          case 16: {
            IsGlobal = input.ReadBool();
            break;
          }
          case 26: {
            collidersIds_.AddEntriesFrom(ref input, _repeated_collidersIds_codec);
            break;
          }
          case 37: {
            BlendDistance = input.ReadFloat();
            break;
          }
          case 45: {
            Weight = input.ReadFloat();
            break;
          }
          case 53: {
            Priority = input.ReadFloat();
            break;
          }
          case 58: {
            if (sharedProfileId_ == null) {
              SharedProfileId = new global::PLUME.Sample.Unity.AssetIdentifier();
            }
            input.ReadMessage(SharedProfileId);
            break;
          }
        }
      }
    }
    #endif

  }

  [global::System.Diagnostics.DebuggerDisplayAttribute("{ToString(),nq}")]
  public sealed partial class VolumeUpdateEnabled : pb::IMessage<VolumeUpdateEnabled>
  #if !GOOGLE_PROTOBUF_REFSTRUCT_COMPATIBILITY_MODE
      , pb::IBufferMessage
  #endif
  {
    private static readonly pb::MessageParser<VolumeUpdateEnabled> _parser = new pb::MessageParser<VolumeUpdateEnabled>(() => new VolumeUpdateEnabled());
    private pb::UnknownFieldSet _unknownFields;
    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    [global::System.CodeDom.Compiler.GeneratedCode("protoc", null)]
    public static pb::MessageParser<VolumeUpdateEnabled> Parser { get { return _parser; } }

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    [global::System.CodeDom.Compiler.GeneratedCode("protoc", null)]
    public static pbr::MessageDescriptor Descriptor {
      get { return global::PLUME.Sample.Unity.URP.VolumeReflection.Descriptor.MessageTypes[2]; }
    }

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    [global::System.CodeDom.Compiler.GeneratedCode("protoc", null)]
    pbr::MessageDescriptor pb::IMessage.Descriptor {
      get { return Descriptor; }
    }

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    [global::System.CodeDom.Compiler.GeneratedCode("protoc", null)]
    public VolumeUpdateEnabled() {
      OnConstruction();
    }

    partial void OnConstruction();

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    [global::System.CodeDom.Compiler.GeneratedCode("protoc", null)]
    public VolumeUpdateEnabled(VolumeUpdateEnabled other) : this() {
      id_ = other.id_ != null ? other.id_.Clone() : null;
      enabled_ = other.enabled_;
      _unknownFields = pb::UnknownFieldSet.Clone(other._unknownFields);
    }

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    [global::System.CodeDom.Compiler.GeneratedCode("protoc", null)]
    public VolumeUpdateEnabled Clone() {
      return new VolumeUpdateEnabled(this);
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

    /// <summary>Field number for the "enabled" field.</summary>
    public const int EnabledFieldNumber = 2;
    private bool enabled_;
    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    [global::System.CodeDom.Compiler.GeneratedCode("protoc", null)]
    public bool Enabled {
      get { return enabled_; }
      set {
        enabled_ = value;
      }
    }

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    [global::System.CodeDom.Compiler.GeneratedCode("protoc", null)]
    public override bool Equals(object other) {
      return Equals(other as VolumeUpdateEnabled);
    }

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    [global::System.CodeDom.Compiler.GeneratedCode("protoc", null)]
    public bool Equals(VolumeUpdateEnabled other) {
      if (ReferenceEquals(other, null)) {
        return false;
      }
      if (ReferenceEquals(other, this)) {
        return true;
      }
      if (!object.Equals(Id, other.Id)) return false;
      if (Enabled != other.Enabled) return false;
      return Equals(_unknownFields, other._unknownFields);
    }

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    [global::System.CodeDom.Compiler.GeneratedCode("protoc", null)]
    public override int GetHashCode() {
      int hash = 1;
      if (id_ != null) hash ^= Id.GetHashCode();
      if (Enabled != false) hash ^= Enabled.GetHashCode();
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
      if (Enabled != false) {
        output.WriteRawTag(16);
        output.WriteBool(Enabled);
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
      if (Enabled != false) {
        output.WriteRawTag(16);
        output.WriteBool(Enabled);
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
      if (Enabled != false) {
        size += 1 + 1;
      }
      if (_unknownFields != null) {
        size += _unknownFields.CalculateSize();
      }
      return size;
    }

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    [global::System.CodeDom.Compiler.GeneratedCode("protoc", null)]
    public void MergeFrom(VolumeUpdateEnabled other) {
      if (other == null) {
        return;
      }
      if (other.id_ != null) {
        if (id_ == null) {
          Id = new global::PLUME.Sample.Unity.ComponentIdentifier();
        }
        Id.MergeFrom(other.Id);
      }
      if (other.Enabled != false) {
        Enabled = other.Enabled;
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
          case 16: {
            Enabled = input.ReadBool();
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
          case 16: {
            Enabled = input.ReadBool();
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
