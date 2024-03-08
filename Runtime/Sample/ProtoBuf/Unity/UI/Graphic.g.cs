// <auto-generated>
//     Generated by the protocol buffer compiler.  DO NOT EDIT!
//     source: unity/ui/graphic.proto
// </auto-generated>
#pragma warning disable 1591, 0612, 3021, 8981
#region Designer generated code

using pb = global::Google.Protobuf;
using pbc = global::Google.Protobuf.Collections;
using pbr = global::Google.Protobuf.Reflection;
using scg = global::System.Collections.Generic;
namespace PLUME.Sample.Unity.UI {

  /// <summary>Holder for reflection information generated from unity/ui/graphic.proto</summary>
  public static partial class GraphicReflection {

    #region Descriptor
    /// <summary>File descriptor for unity/ui/graphic.proto</summary>
    public static pbr::FileDescriptor Descriptor {
      get { return descriptor; }
    }
    private static pbr::FileDescriptor descriptor;

    static GraphicReflection() {
      byte[] descriptorData = global::System.Convert.FromBase64String(
          string.Concat(
            "ChZ1bml0eS91aS9ncmFwaGljLnByb3RvEhJwbHVtZS5zYW1wbGUudW5pdHka",
            "F3VuaXR5L2lkZW50aWZpZXJzLnByb3RvGhRjb21tb24vdmVjdG9yNC5wcm90",
            "bxoSY29tbW9uL2NvbG9yLnByb3RvIs0CCg1HcmFwaGljVXBkYXRlEjMKAmlk",
            "GAEgASgLMicucGx1bWUuc2FtcGxlLnVuaXR5LkNvbXBvbmVudElkZW50aWZp",
            "ZXISLgoFY29sb3IYAiABKAsyGi5wbHVtZS5zYW1wbGUuY29tbW9uLkNvbG9y",
            "SACIAQESGwoOcmF5Y2FzdF90YXJnZXQYAyABKAhIAYgBARI6Cg9yYXljYXN0",
            "X3BhZGRpbmcYBCABKAsyHC5wbHVtZS5zYW1wbGUuY29tbW9uLlZlY3RvcjRI",
            "AogBARI9CgttYXRlcmlhbF9pZBgFIAEoCzIjLnBsdW1lLnNhbXBsZS51bml0",
            "eS5Bc3NldElkZW50aWZpZXJIA4gBAUIICgZfY29sb3JCEQoPX3JheWNhc3Rf",
            "dGFyZ2V0QhIKEF9yYXljYXN0X3BhZGRpbmdCDgoMX21hdGVyaWFsX2lkQhiq",
            "AhVQTFVNRS5TYW1wbGUuVW5pdHkuVUliBnByb3RvMw=="));
      descriptor = pbr::FileDescriptor.FromGeneratedCode(descriptorData,
          new pbr::FileDescriptor[] { global::PLUME.Sample.Unity.IdentifiersReflection.Descriptor, global::PLUME.Sample.Common.Vector4Reflection.Descriptor, global::PLUME.Sample.Common.ColorReflection.Descriptor, },
          new pbr::GeneratedClrTypeInfo(null, null, new pbr::GeneratedClrTypeInfo[] {
            new pbr::GeneratedClrTypeInfo(typeof(global::PLUME.Sample.Unity.UI.GraphicUpdate), global::PLUME.Sample.Unity.UI.GraphicUpdate.Parser, new[]{ "Id", "Color", "RaycastTarget", "RaycastPadding", "MaterialId" }, new[]{ "Color", "RaycastTarget", "RaycastPadding", "MaterialId" }, null, null, null)
          }));
    }
    #endregion

  }
  #region Messages
  public sealed partial class GraphicUpdate : pb::IMessage<GraphicUpdate>
  #if !GOOGLE_PROTOBUF_REFSTRUCT_COMPATIBILITY_MODE
      , pb::IBufferMessage
  #endif
  {
    private static readonly pb::MessageParser<GraphicUpdate> _parser = new pb::MessageParser<GraphicUpdate>(() => new GraphicUpdate());
    private pb::UnknownFieldSet _unknownFields;
    private int _hasBits0;
    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    [global::System.CodeDom.Compiler.GeneratedCode("protoc", null)]
    public static pb::MessageParser<GraphicUpdate> Parser { get { return _parser; } }

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    [global::System.CodeDom.Compiler.GeneratedCode("protoc", null)]
    public static pbr::MessageDescriptor Descriptor {
      get { return global::PLUME.Sample.Unity.UI.GraphicReflection.Descriptor.MessageTypes[0]; }
    }

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    [global::System.CodeDom.Compiler.GeneratedCode("protoc", null)]
    pbr::MessageDescriptor pb::IMessage.Descriptor {
      get { return Descriptor; }
    }

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    [global::System.CodeDom.Compiler.GeneratedCode("protoc", null)]
    public GraphicUpdate() {
      OnConstruction();
    }

    partial void OnConstruction();

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    [global::System.CodeDom.Compiler.GeneratedCode("protoc", null)]
    public GraphicUpdate(GraphicUpdate other) : this() {
      _hasBits0 = other._hasBits0;
      id_ = other.id_ != null ? other.id_.Clone() : null;
      color_ = other.color_ != null ? other.color_.Clone() : null;
      raycastTarget_ = other.raycastTarget_;
      raycastPadding_ = other.raycastPadding_ != null ? other.raycastPadding_.Clone() : null;
      materialId_ = other.materialId_ != null ? other.materialId_.Clone() : null;
      _unknownFields = pb::UnknownFieldSet.Clone(other._unknownFields);
    }

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    [global::System.CodeDom.Compiler.GeneratedCode("protoc", null)]
    public GraphicUpdate Clone() {
      return new GraphicUpdate(this);
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

    /// <summary>Field number for the "color" field.</summary>
    public const int ColorFieldNumber = 2;
    private global::PLUME.Sample.Common.Color color_;
    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    [global::System.CodeDom.Compiler.GeneratedCode("protoc", null)]
    public global::PLUME.Sample.Common.Color Color {
      get { return color_; }
      set {
        color_ = value;
      }
    }

    /// <summary>Field number for the "raycast_target" field.</summary>
    public const int RaycastTargetFieldNumber = 3;
    private readonly static bool RaycastTargetDefaultValue = false;

    private bool raycastTarget_;
    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    [global::System.CodeDom.Compiler.GeneratedCode("protoc", null)]
    public bool RaycastTarget {
      get { if ((_hasBits0 & 1) != 0) { return raycastTarget_; } else { return RaycastTargetDefaultValue; } }
      set {
        _hasBits0 |= 1;
        raycastTarget_ = value;
      }
    }
    /// <summary>Gets whether the "raycast_target" field is set</summary>
    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    [global::System.CodeDom.Compiler.GeneratedCode("protoc", null)]
    public bool HasRaycastTarget {
      get { return (_hasBits0 & 1) != 0; }
    }
    /// <summary>Clears the value of the "raycast_target" field</summary>
    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    [global::System.CodeDom.Compiler.GeneratedCode("protoc", null)]
    public void ClearRaycastTarget() {
      _hasBits0 &= ~1;
    }

    /// <summary>Field number for the "raycast_padding" field.</summary>
    public const int RaycastPaddingFieldNumber = 4;
    private global::PLUME.Sample.Common.Vector4 raycastPadding_;
    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    [global::System.CodeDom.Compiler.GeneratedCode("protoc", null)]
    public global::PLUME.Sample.Common.Vector4 RaycastPadding {
      get { return raycastPadding_; }
      set {
        raycastPadding_ = value;
      }
    }

    /// <summary>Field number for the "material_id" field.</summary>
    public const int MaterialIdFieldNumber = 5;
    private global::PLUME.Sample.Unity.AssetIdentifier materialId_;
    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    [global::System.CodeDom.Compiler.GeneratedCode("protoc", null)]
    public global::PLUME.Sample.Unity.AssetIdentifier MaterialId {
      get { return materialId_; }
      set {
        materialId_ = value;
      }
    }

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    [global::System.CodeDom.Compiler.GeneratedCode("protoc", null)]
    public override bool Equals(object other) {
      return Equals(other as GraphicUpdate);
    }

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    [global::System.CodeDom.Compiler.GeneratedCode("protoc", null)]
    public bool Equals(GraphicUpdate other) {
      if (ReferenceEquals(other, null)) {
        return false;
      }
      if (ReferenceEquals(other, this)) {
        return true;
      }
      if (!object.Equals(Id, other.Id)) return false;
      if (!object.Equals(Color, other.Color)) return false;
      if (RaycastTarget != other.RaycastTarget) return false;
      if (!object.Equals(RaycastPadding, other.RaycastPadding)) return false;
      if (!object.Equals(MaterialId, other.MaterialId)) return false;
      return Equals(_unknownFields, other._unknownFields);
    }

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    [global::System.CodeDom.Compiler.GeneratedCode("protoc", null)]
    public override int GetHashCode() {
      int hash = 1;
      if (id_ != null) hash ^= Id.GetHashCode();
      if (color_ != null) hash ^= Color.GetHashCode();
      if (HasRaycastTarget) hash ^= RaycastTarget.GetHashCode();
      if (raycastPadding_ != null) hash ^= RaycastPadding.GetHashCode();
      if (materialId_ != null) hash ^= MaterialId.GetHashCode();
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
      if (color_ != null) {
        output.WriteRawTag(18);
        output.WriteMessage(Color);
      }
      if (HasRaycastTarget) {
        output.WriteRawTag(24);
        output.WriteBool(RaycastTarget);
      }
      if (raycastPadding_ != null) {
        output.WriteRawTag(34);
        output.WriteMessage(RaycastPadding);
      }
      if (materialId_ != null) {
        output.WriteRawTag(42);
        output.WriteMessage(MaterialId);
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
      if (color_ != null) {
        output.WriteRawTag(18);
        output.WriteMessage(Color);
      }
      if (HasRaycastTarget) {
        output.WriteRawTag(24);
        output.WriteBool(RaycastTarget);
      }
      if (raycastPadding_ != null) {
        output.WriteRawTag(34);
        output.WriteMessage(RaycastPadding);
      }
      if (materialId_ != null) {
        output.WriteRawTag(42);
        output.WriteMessage(MaterialId);
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
      if (color_ != null) {
        size += 1 + pb::CodedOutputStream.ComputeMessageSize(Color);
      }
      if (HasRaycastTarget) {
        size += 1 + 1;
      }
      if (raycastPadding_ != null) {
        size += 1 + pb::CodedOutputStream.ComputeMessageSize(RaycastPadding);
      }
      if (materialId_ != null) {
        size += 1 + pb::CodedOutputStream.ComputeMessageSize(MaterialId);
      }
      if (_unknownFields != null) {
        size += _unknownFields.CalculateSize();
      }
      return size;
    }

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    [global::System.CodeDom.Compiler.GeneratedCode("protoc", null)]
    public void MergeFrom(GraphicUpdate other) {
      if (other == null) {
        return;
      }
      if (other.id_ != null) {
        if (id_ == null) {
          Id = new global::PLUME.Sample.Unity.ComponentIdentifier();
        }
        Id.MergeFrom(other.Id);
      }
      if (other.color_ != null) {
        if (color_ == null) {
          Color = new global::PLUME.Sample.Common.Color();
        }
        Color.MergeFrom(other.Color);
      }
      if (other.HasRaycastTarget) {
        RaycastTarget = other.RaycastTarget;
      }
      if (other.raycastPadding_ != null) {
        if (raycastPadding_ == null) {
          RaycastPadding = new global::PLUME.Sample.Common.Vector4();
        }
        RaycastPadding.MergeFrom(other.RaycastPadding);
      }
      if (other.materialId_ != null) {
        if (materialId_ == null) {
          MaterialId = new global::PLUME.Sample.Unity.AssetIdentifier();
        }
        MaterialId.MergeFrom(other.MaterialId);
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
            if (color_ == null) {
              Color = new global::PLUME.Sample.Common.Color();
            }
            input.ReadMessage(Color);
            break;
          }
          case 24: {
            RaycastTarget = input.ReadBool();
            break;
          }
          case 34: {
            if (raycastPadding_ == null) {
              RaycastPadding = new global::PLUME.Sample.Common.Vector4();
            }
            input.ReadMessage(RaycastPadding);
            break;
          }
          case 42: {
            if (materialId_ == null) {
              MaterialId = new global::PLUME.Sample.Unity.AssetIdentifier();
            }
            input.ReadMessage(MaterialId);
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
            if (color_ == null) {
              Color = new global::PLUME.Sample.Common.Color();
            }
            input.ReadMessage(Color);
            break;
          }
          case 24: {
            RaycastTarget = input.ReadBool();
            break;
          }
          case 34: {
            if (raycastPadding_ == null) {
              RaycastPadding = new global::PLUME.Sample.Common.Vector4();
            }
            input.ReadMessage(RaycastPadding);
            break;
          }
          case 42: {
            if (materialId_ == null) {
              MaterialId = new global::PLUME.Sample.Unity.AssetIdentifier();
            }
            input.ReadMessage(MaterialId);
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
