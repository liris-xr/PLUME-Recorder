// <auto-generated>
//     Generated by the protocol buffer compiler.  DO NOT EDIT!
//     source: plume/sample/unity/settings/urp_global_settings.proto
// </auto-generated>
#pragma warning disable 1591, 0612, 3021, 8981
#region Designer generated code

using pb = global::Google.Protobuf;
using pbc = global::Google.Protobuf.Collections;
using pbr = global::Google.Protobuf.Reflection;
using scg = global::System.Collections.Generic;
namespace PLUME.Sample.Unity.Settings {

  /// <summary>Holder for reflection information generated from plume/sample/unity/settings/urp_global_settings.proto</summary>
  public static partial class UrpGlobalSettingsReflection {

    #region Descriptor
    /// <summary>File descriptor for plume/sample/unity/settings/urp_global_settings.proto</summary>
    public static pbr::FileDescriptor Descriptor {
      get { return descriptor; }
    }
    private static pbr::FileDescriptor descriptor;

    static UrpGlobalSettingsReflection() {
      byte[] descriptorData = global::System.Convert.FromBase64String(
          string.Concat(
            "CjVwbHVtZS9zYW1wbGUvdW5pdHkvc2V0dGluZ3MvdXJwX2dsb2JhbF9zZXR0",
            "aW5ncy5wcm90bxIbcGx1bWUuc2FtcGxlLnVuaXR5LnNldHRpbmdzGiRwbHVt",
            "ZS9zYW1wbGUvdW5pdHkvaWRlbnRpZmllcnMucHJvdG8ikQEKF1VSUEdsb2Jh",
            "bFNldHRpbmdzVXBkYXRlEhIKBG5hbWUYASABKAlSBG5hbWUSTwoOc2V0dGlu",
            "Z3NfYXNzZXQYAiABKAsyIy5wbHVtZS5zYW1wbGUudW5pdHkuQXNzZXRJZGVu",
            "dGlmaWVySABSDXNldHRpbmdzQXNzZXSIAQFCEQoPX3NldHRpbmdzX2Fzc2V0",
            "Qh6qAhtQTFVNRS5TYW1wbGUuVW5pdHkuU2V0dGluZ3NiBnByb3RvMw=="));
      descriptor = pbr::FileDescriptor.FromGeneratedCode(descriptorData,
          new pbr::FileDescriptor[] { global::PLUME.Sample.Unity.IdentifiersReflection.Descriptor, },
          new pbr::GeneratedClrTypeInfo(null, null, new pbr::GeneratedClrTypeInfo[] {
            new pbr::GeneratedClrTypeInfo(typeof(global::PLUME.Sample.Unity.Settings.URPGlobalSettingsUpdate), global::PLUME.Sample.Unity.Settings.URPGlobalSettingsUpdate.Parser, new[]{ "Name", "SettingsAsset" }, new[]{ "SettingsAsset" }, null, null, null)
          }));
    }
    #endregion

  }
  #region Messages
  /// <summary>
  /// Corresponds to the properties of a quality level defined in Project Settings > Graphics > URP Global Settings
  /// </summary>
  [global::System.Diagnostics.DebuggerDisplayAttribute("{ToString(),nq}")]
  public sealed partial class URPGlobalSettingsUpdate : pb::IMessage<URPGlobalSettingsUpdate>
  #if !GOOGLE_PROTOBUF_REFSTRUCT_COMPATIBILITY_MODE
      , pb::IBufferMessage
  #endif
  {
    private static readonly pb::MessageParser<URPGlobalSettingsUpdate> _parser = new pb::MessageParser<URPGlobalSettingsUpdate>(() => new URPGlobalSettingsUpdate());
    private pb::UnknownFieldSet _unknownFields;
    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    [global::System.CodeDom.Compiler.GeneratedCode("protoc", null)]
    public static pb::MessageParser<URPGlobalSettingsUpdate> Parser { get { return _parser; } }

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    [global::System.CodeDom.Compiler.GeneratedCode("protoc", null)]
    public static pbr::MessageDescriptor Descriptor {
      get { return global::PLUME.Sample.Unity.Settings.UrpGlobalSettingsReflection.Descriptor.MessageTypes[0]; }
    }

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    [global::System.CodeDom.Compiler.GeneratedCode("protoc", null)]
    pbr::MessageDescriptor pb::IMessage.Descriptor {
      get { return Descriptor; }
    }

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    [global::System.CodeDom.Compiler.GeneratedCode("protoc", null)]
    public URPGlobalSettingsUpdate() {
      OnConstruction();
    }

    partial void OnConstruction();

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    [global::System.CodeDom.Compiler.GeneratedCode("protoc", null)]
    public URPGlobalSettingsUpdate(URPGlobalSettingsUpdate other) : this() {
      name_ = other.name_;
      settingsAsset_ = other.settingsAsset_ != null ? other.settingsAsset_.Clone() : null;
      _unknownFields = pb::UnknownFieldSet.Clone(other._unknownFields);
    }

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    [global::System.CodeDom.Compiler.GeneratedCode("protoc", null)]
    public URPGlobalSettingsUpdate Clone() {
      return new URPGlobalSettingsUpdate(this);
    }

    /// <summary>Field number for the "name" field.</summary>
    public const int NameFieldNumber = 1;
    private string name_ = "";
    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    [global::System.CodeDom.Compiler.GeneratedCode("protoc", null)]
    public string Name {
      get { return name_; }
      set {
        name_ = pb::ProtoPreconditions.CheckNotNull(value, "value");
      }
    }

    /// <summary>Field number for the "settings_asset" field.</summary>
    public const int SettingsAssetFieldNumber = 2;
    private global::PLUME.Sample.Unity.AssetIdentifier settingsAsset_;
    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    [global::System.CodeDom.Compiler.GeneratedCode("protoc", null)]
    public global::PLUME.Sample.Unity.AssetIdentifier SettingsAsset {
      get { return settingsAsset_; }
      set {
        settingsAsset_ = value;
      }
    }

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    [global::System.CodeDom.Compiler.GeneratedCode("protoc", null)]
    public override bool Equals(object other) {
      return Equals(other as URPGlobalSettingsUpdate);
    }

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    [global::System.CodeDom.Compiler.GeneratedCode("protoc", null)]
    public bool Equals(URPGlobalSettingsUpdate other) {
      if (ReferenceEquals(other, null)) {
        return false;
      }
      if (ReferenceEquals(other, this)) {
        return true;
      }
      if (Name != other.Name) return false;
      if (!object.Equals(SettingsAsset, other.SettingsAsset)) return false;
      return Equals(_unknownFields, other._unknownFields);
    }

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    [global::System.CodeDom.Compiler.GeneratedCode("protoc", null)]
    public override int GetHashCode() {
      int hash = 1;
      if (Name.Length != 0) hash ^= Name.GetHashCode();
      if (settingsAsset_ != null) hash ^= SettingsAsset.GetHashCode();
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
      if (Name.Length != 0) {
        output.WriteRawTag(10);
        output.WriteString(Name);
      }
      if (settingsAsset_ != null) {
        output.WriteRawTag(18);
        output.WriteMessage(SettingsAsset);
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
      if (Name.Length != 0) {
        output.WriteRawTag(10);
        output.WriteString(Name);
      }
      if (settingsAsset_ != null) {
        output.WriteRawTag(18);
        output.WriteMessage(SettingsAsset);
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
      if (Name.Length != 0) {
        size += 1 + pb::CodedOutputStream.ComputeStringSize(Name);
      }
      if (settingsAsset_ != null) {
        size += 1 + pb::CodedOutputStream.ComputeMessageSize(SettingsAsset);
      }
      if (_unknownFields != null) {
        size += _unknownFields.CalculateSize();
      }
      return size;
    }

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    [global::System.CodeDom.Compiler.GeneratedCode("protoc", null)]
    public void MergeFrom(URPGlobalSettingsUpdate other) {
      if (other == null) {
        return;
      }
      if (other.Name.Length != 0) {
        Name = other.Name;
      }
      if (other.settingsAsset_ != null) {
        if (settingsAsset_ == null) {
          SettingsAsset = new global::PLUME.Sample.Unity.AssetIdentifier();
        }
        SettingsAsset.MergeFrom(other.SettingsAsset);
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
            Name = input.ReadString();
            break;
          }
          case 18: {
            if (settingsAsset_ == null) {
              SettingsAsset = new global::PLUME.Sample.Unity.AssetIdentifier();
            }
            input.ReadMessage(SettingsAsset);
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
            Name = input.ReadString();
            break;
          }
          case 18: {
            if (settingsAsset_ == null) {
              SettingsAsset = new global::PLUME.Sample.Unity.AssetIdentifier();
            }
            input.ReadMessage(SettingsAsset);
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
