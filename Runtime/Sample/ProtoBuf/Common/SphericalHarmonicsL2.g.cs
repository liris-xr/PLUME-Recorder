// <auto-generated>
//     Generated by the protocol buffer compiler.  DO NOT EDIT!
//     source: plume/sample/common/spherical_harmonics_l2.proto
// </auto-generated>
#pragma warning disable 1591, 0612, 3021, 8981
#region Designer generated code

using pb = global::Google.Protobuf;
using pbc = global::Google.Protobuf.Collections;
using pbr = global::Google.Protobuf.Reflection;
using scg = global::System.Collections.Generic;
namespace PLUME.Sample.Common {

  /// <summary>Holder for reflection information generated from plume/sample/common/spherical_harmonics_l2.proto</summary>
  public static partial class SphericalHarmonicsL2Reflection {

    #region Descriptor
    /// <summary>File descriptor for plume/sample/common/spherical_harmonics_l2.proto</summary>
    public static pbr::FileDescriptor Descriptor {
      get { return descriptor; }
    }
    private static pbr::FileDescriptor descriptor;

    static SphericalHarmonicsL2Reflection() {
      byte[] descriptorData = global::System.Convert.FromBase64String(
          string.Concat(
            "CjBwbHVtZS9zYW1wbGUvY29tbW9uL3NwaGVyaWNhbF9oYXJtb25pY3NfbDIu",
            "cHJvdG8SE3BsdW1lLnNhbXBsZS5jb21tb24isgQKFFNwaGVyaWNhbEhhcm1v",
            "bmljc0wyEhIKBHNocjAYASABKAJSBHNocjASEgoEc2hyMRgCIAEoAlIEc2hy",
            "MRISCgRzaHIyGAMgASgCUgRzaHIyEhIKBHNocjMYBCABKAJSBHNocjMSEgoE",
            "c2hyNBgFIAEoAlIEc2hyNBISCgRzaHI1GAYgASgCUgRzaHI1EhIKBHNocjYY",
            "ByABKAJSBHNocjYSEgoEc2hyNxgIIAEoAlIEc2hyNxISCgRzaHI4GAkgASgC",
            "UgRzaHI4EhIKBHNoZzAYCiABKAJSBHNoZzASEgoEc2hnMRgLIAEoAlIEc2hn",
            "MRISCgRzaGcyGAwgASgCUgRzaGcyEhIKBHNoZzMYDSABKAJSBHNoZzMSEgoE",
            "c2hnNBgOIAEoAlIEc2hnNBISCgRzaGc1GA8gASgCUgRzaGc1EhIKBHNoZzYY",
            "ECABKAJSBHNoZzYSEgoEc2hnNxgRIAEoAlIEc2hnNxISCgRzaGc4GBIgASgC",
            "UgRzaGc4EhIKBHNoYjAYEyABKAJSBHNoYjASEgoEc2hiMRgUIAEoAlIEc2hi",
            "MRISCgRzaGIyGBUgASgCUgRzaGIyEhIKBHNoYjMYFiABKAJSBHNoYjMSEgoE",
            "c2hiNBgXIAEoAlIEc2hiNBISCgRzaGI1GBggASgCUgRzaGI1EhIKBHNoYjYY",
            "GSABKAJSBHNoYjYSEgoEc2hiNxgaIAEoAlIEc2hiNxISCgRzaGI4GBsgASgC",
            "UgRzaGI4QhaqAhNQTFVNRS5TYW1wbGUuQ29tbW9uYgZwcm90bzM="));
      descriptor = pbr::FileDescriptor.FromGeneratedCode(descriptorData,
          new pbr::FileDescriptor[] { },
          new pbr::GeneratedClrTypeInfo(null, null, new pbr::GeneratedClrTypeInfo[] {
            new pbr::GeneratedClrTypeInfo(typeof(global::PLUME.Sample.Common.SphericalHarmonicsL2), global::PLUME.Sample.Common.SphericalHarmonicsL2.Parser, new[]{ "Shr0", "Shr1", "Shr2", "Shr3", "Shr4", "Shr5", "Shr6", "Shr7", "Shr8", "Shg0", "Shg1", "Shg2", "Shg3", "Shg4", "Shg5", "Shg6", "Shg7", "Shg8", "Shb0", "Shb1", "Shb2", "Shb3", "Shb4", "Shb5", "Shb6", "Shb7", "Shb8" }, null, null, null, null)
          }));
    }
    #endregion

  }
  #region Messages
  [global::System.Diagnostics.DebuggerDisplayAttribute("{ToString(),nq}")]
  public sealed partial class SphericalHarmonicsL2 : pb::IMessage<SphericalHarmonicsL2>
  #if !GOOGLE_PROTOBUF_REFSTRUCT_COMPATIBILITY_MODE
      , pb::IBufferMessage
  #endif
  {
    private static readonly pb::MessageParser<SphericalHarmonicsL2> _parser = new pb::MessageParser<SphericalHarmonicsL2>(() => new SphericalHarmonicsL2());
    private pb::UnknownFieldSet _unknownFields;
    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    [global::System.CodeDom.Compiler.GeneratedCode("protoc", null)]
    public static pb::MessageParser<SphericalHarmonicsL2> Parser { get { return _parser; } }

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    [global::System.CodeDom.Compiler.GeneratedCode("protoc", null)]
    public static pbr::MessageDescriptor Descriptor {
      get { return global::PLUME.Sample.Common.SphericalHarmonicsL2Reflection.Descriptor.MessageTypes[0]; }
    }

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    [global::System.CodeDom.Compiler.GeneratedCode("protoc", null)]
    pbr::MessageDescriptor pb::IMessage.Descriptor {
      get { return Descriptor; }
    }

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    [global::System.CodeDom.Compiler.GeneratedCode("protoc", null)]
    public SphericalHarmonicsL2() {
      OnConstruction();
    }

    partial void OnConstruction();

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    [global::System.CodeDom.Compiler.GeneratedCode("protoc", null)]
    public SphericalHarmonicsL2(SphericalHarmonicsL2 other) : this() {
      shr0_ = other.shr0_;
      shr1_ = other.shr1_;
      shr2_ = other.shr2_;
      shr3_ = other.shr3_;
      shr4_ = other.shr4_;
      shr5_ = other.shr5_;
      shr6_ = other.shr6_;
      shr7_ = other.shr7_;
      shr8_ = other.shr8_;
      shg0_ = other.shg0_;
      shg1_ = other.shg1_;
      shg2_ = other.shg2_;
      shg3_ = other.shg3_;
      shg4_ = other.shg4_;
      shg5_ = other.shg5_;
      shg6_ = other.shg6_;
      shg7_ = other.shg7_;
      shg8_ = other.shg8_;
      shb0_ = other.shb0_;
      shb1_ = other.shb1_;
      shb2_ = other.shb2_;
      shb3_ = other.shb3_;
      shb4_ = other.shb4_;
      shb5_ = other.shb5_;
      shb6_ = other.shb6_;
      shb7_ = other.shb7_;
      shb8_ = other.shb8_;
      _unknownFields = pb::UnknownFieldSet.Clone(other._unknownFields);
    }

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    [global::System.CodeDom.Compiler.GeneratedCode("protoc", null)]
    public SphericalHarmonicsL2 Clone() {
      return new SphericalHarmonicsL2(this);
    }

    /// <summary>Field number for the "shr0" field.</summary>
    public const int Shr0FieldNumber = 1;
    private float shr0_;
    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    [global::System.CodeDom.Compiler.GeneratedCode("protoc", null)]
    public float Shr0 {
      get { return shr0_; }
      set {
        shr0_ = value;
      }
    }

    /// <summary>Field number for the "shr1" field.</summary>
    public const int Shr1FieldNumber = 2;
    private float shr1_;
    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    [global::System.CodeDom.Compiler.GeneratedCode("protoc", null)]
    public float Shr1 {
      get { return shr1_; }
      set {
        shr1_ = value;
      }
    }

    /// <summary>Field number for the "shr2" field.</summary>
    public const int Shr2FieldNumber = 3;
    private float shr2_;
    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    [global::System.CodeDom.Compiler.GeneratedCode("protoc", null)]
    public float Shr2 {
      get { return shr2_; }
      set {
        shr2_ = value;
      }
    }

    /// <summary>Field number for the "shr3" field.</summary>
    public const int Shr3FieldNumber = 4;
    private float shr3_;
    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    [global::System.CodeDom.Compiler.GeneratedCode("protoc", null)]
    public float Shr3 {
      get { return shr3_; }
      set {
        shr3_ = value;
      }
    }

    /// <summary>Field number for the "shr4" field.</summary>
    public const int Shr4FieldNumber = 5;
    private float shr4_;
    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    [global::System.CodeDom.Compiler.GeneratedCode("protoc", null)]
    public float Shr4 {
      get { return shr4_; }
      set {
        shr4_ = value;
      }
    }

    /// <summary>Field number for the "shr5" field.</summary>
    public const int Shr5FieldNumber = 6;
    private float shr5_;
    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    [global::System.CodeDom.Compiler.GeneratedCode("protoc", null)]
    public float Shr5 {
      get { return shr5_; }
      set {
        shr5_ = value;
      }
    }

    /// <summary>Field number for the "shr6" field.</summary>
    public const int Shr6FieldNumber = 7;
    private float shr6_;
    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    [global::System.CodeDom.Compiler.GeneratedCode("protoc", null)]
    public float Shr6 {
      get { return shr6_; }
      set {
        shr6_ = value;
      }
    }

    /// <summary>Field number for the "shr7" field.</summary>
    public const int Shr7FieldNumber = 8;
    private float shr7_;
    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    [global::System.CodeDom.Compiler.GeneratedCode("protoc", null)]
    public float Shr7 {
      get { return shr7_; }
      set {
        shr7_ = value;
      }
    }

    /// <summary>Field number for the "shr8" field.</summary>
    public const int Shr8FieldNumber = 9;
    private float shr8_;
    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    [global::System.CodeDom.Compiler.GeneratedCode("protoc", null)]
    public float Shr8 {
      get { return shr8_; }
      set {
        shr8_ = value;
      }
    }

    /// <summary>Field number for the "shg0" field.</summary>
    public const int Shg0FieldNumber = 10;
    private float shg0_;
    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    [global::System.CodeDom.Compiler.GeneratedCode("protoc", null)]
    public float Shg0 {
      get { return shg0_; }
      set {
        shg0_ = value;
      }
    }

    /// <summary>Field number for the "shg1" field.</summary>
    public const int Shg1FieldNumber = 11;
    private float shg1_;
    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    [global::System.CodeDom.Compiler.GeneratedCode("protoc", null)]
    public float Shg1 {
      get { return shg1_; }
      set {
        shg1_ = value;
      }
    }

    /// <summary>Field number for the "shg2" field.</summary>
    public const int Shg2FieldNumber = 12;
    private float shg2_;
    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    [global::System.CodeDom.Compiler.GeneratedCode("protoc", null)]
    public float Shg2 {
      get { return shg2_; }
      set {
        shg2_ = value;
      }
    }

    /// <summary>Field number for the "shg3" field.</summary>
    public const int Shg3FieldNumber = 13;
    private float shg3_;
    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    [global::System.CodeDom.Compiler.GeneratedCode("protoc", null)]
    public float Shg3 {
      get { return shg3_; }
      set {
        shg3_ = value;
      }
    }

    /// <summary>Field number for the "shg4" field.</summary>
    public const int Shg4FieldNumber = 14;
    private float shg4_;
    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    [global::System.CodeDom.Compiler.GeneratedCode("protoc", null)]
    public float Shg4 {
      get { return shg4_; }
      set {
        shg4_ = value;
      }
    }

    /// <summary>Field number for the "shg5" field.</summary>
    public const int Shg5FieldNumber = 15;
    private float shg5_;
    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    [global::System.CodeDom.Compiler.GeneratedCode("protoc", null)]
    public float Shg5 {
      get { return shg5_; }
      set {
        shg5_ = value;
      }
    }

    /// <summary>Field number for the "shg6" field.</summary>
    public const int Shg6FieldNumber = 16;
    private float shg6_;
    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    [global::System.CodeDom.Compiler.GeneratedCode("protoc", null)]
    public float Shg6 {
      get { return shg6_; }
      set {
        shg6_ = value;
      }
    }

    /// <summary>Field number for the "shg7" field.</summary>
    public const int Shg7FieldNumber = 17;
    private float shg7_;
    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    [global::System.CodeDom.Compiler.GeneratedCode("protoc", null)]
    public float Shg7 {
      get { return shg7_; }
      set {
        shg7_ = value;
      }
    }

    /// <summary>Field number for the "shg8" field.</summary>
    public const int Shg8FieldNumber = 18;
    private float shg8_;
    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    [global::System.CodeDom.Compiler.GeneratedCode("protoc", null)]
    public float Shg8 {
      get { return shg8_; }
      set {
        shg8_ = value;
      }
    }

    /// <summary>Field number for the "shb0" field.</summary>
    public const int Shb0FieldNumber = 19;
    private float shb0_;
    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    [global::System.CodeDom.Compiler.GeneratedCode("protoc", null)]
    public float Shb0 {
      get { return shb0_; }
      set {
        shb0_ = value;
      }
    }

    /// <summary>Field number for the "shb1" field.</summary>
    public const int Shb1FieldNumber = 20;
    private float shb1_;
    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    [global::System.CodeDom.Compiler.GeneratedCode("protoc", null)]
    public float Shb1 {
      get { return shb1_; }
      set {
        shb1_ = value;
      }
    }

    /// <summary>Field number for the "shb2" field.</summary>
    public const int Shb2FieldNumber = 21;
    private float shb2_;
    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    [global::System.CodeDom.Compiler.GeneratedCode("protoc", null)]
    public float Shb2 {
      get { return shb2_; }
      set {
        shb2_ = value;
      }
    }

    /// <summary>Field number for the "shb3" field.</summary>
    public const int Shb3FieldNumber = 22;
    private float shb3_;
    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    [global::System.CodeDom.Compiler.GeneratedCode("protoc", null)]
    public float Shb3 {
      get { return shb3_; }
      set {
        shb3_ = value;
      }
    }

    /// <summary>Field number for the "shb4" field.</summary>
    public const int Shb4FieldNumber = 23;
    private float shb4_;
    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    [global::System.CodeDom.Compiler.GeneratedCode("protoc", null)]
    public float Shb4 {
      get { return shb4_; }
      set {
        shb4_ = value;
      }
    }

    /// <summary>Field number for the "shb5" field.</summary>
    public const int Shb5FieldNumber = 24;
    private float shb5_;
    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    [global::System.CodeDom.Compiler.GeneratedCode("protoc", null)]
    public float Shb5 {
      get { return shb5_; }
      set {
        shb5_ = value;
      }
    }

    /// <summary>Field number for the "shb6" field.</summary>
    public const int Shb6FieldNumber = 25;
    private float shb6_;
    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    [global::System.CodeDom.Compiler.GeneratedCode("protoc", null)]
    public float Shb6 {
      get { return shb6_; }
      set {
        shb6_ = value;
      }
    }

    /// <summary>Field number for the "shb7" field.</summary>
    public const int Shb7FieldNumber = 26;
    private float shb7_;
    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    [global::System.CodeDom.Compiler.GeneratedCode("protoc", null)]
    public float Shb7 {
      get { return shb7_; }
      set {
        shb7_ = value;
      }
    }

    /// <summary>Field number for the "shb8" field.</summary>
    public const int Shb8FieldNumber = 27;
    private float shb8_;
    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    [global::System.CodeDom.Compiler.GeneratedCode("protoc", null)]
    public float Shb8 {
      get { return shb8_; }
      set {
        shb8_ = value;
      }
    }

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    [global::System.CodeDom.Compiler.GeneratedCode("protoc", null)]
    public override bool Equals(object other) {
      return Equals(other as SphericalHarmonicsL2);
    }

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    [global::System.CodeDom.Compiler.GeneratedCode("protoc", null)]
    public bool Equals(SphericalHarmonicsL2 other) {
      if (ReferenceEquals(other, null)) {
        return false;
      }
      if (ReferenceEquals(other, this)) {
        return true;
      }
      if (!pbc::ProtobufEqualityComparers.BitwiseSingleEqualityComparer.Equals(Shr0, other.Shr0)) return false;
      if (!pbc::ProtobufEqualityComparers.BitwiseSingleEqualityComparer.Equals(Shr1, other.Shr1)) return false;
      if (!pbc::ProtobufEqualityComparers.BitwiseSingleEqualityComparer.Equals(Shr2, other.Shr2)) return false;
      if (!pbc::ProtobufEqualityComparers.BitwiseSingleEqualityComparer.Equals(Shr3, other.Shr3)) return false;
      if (!pbc::ProtobufEqualityComparers.BitwiseSingleEqualityComparer.Equals(Shr4, other.Shr4)) return false;
      if (!pbc::ProtobufEqualityComparers.BitwiseSingleEqualityComparer.Equals(Shr5, other.Shr5)) return false;
      if (!pbc::ProtobufEqualityComparers.BitwiseSingleEqualityComparer.Equals(Shr6, other.Shr6)) return false;
      if (!pbc::ProtobufEqualityComparers.BitwiseSingleEqualityComparer.Equals(Shr7, other.Shr7)) return false;
      if (!pbc::ProtobufEqualityComparers.BitwiseSingleEqualityComparer.Equals(Shr8, other.Shr8)) return false;
      if (!pbc::ProtobufEqualityComparers.BitwiseSingleEqualityComparer.Equals(Shg0, other.Shg0)) return false;
      if (!pbc::ProtobufEqualityComparers.BitwiseSingleEqualityComparer.Equals(Shg1, other.Shg1)) return false;
      if (!pbc::ProtobufEqualityComparers.BitwiseSingleEqualityComparer.Equals(Shg2, other.Shg2)) return false;
      if (!pbc::ProtobufEqualityComparers.BitwiseSingleEqualityComparer.Equals(Shg3, other.Shg3)) return false;
      if (!pbc::ProtobufEqualityComparers.BitwiseSingleEqualityComparer.Equals(Shg4, other.Shg4)) return false;
      if (!pbc::ProtobufEqualityComparers.BitwiseSingleEqualityComparer.Equals(Shg5, other.Shg5)) return false;
      if (!pbc::ProtobufEqualityComparers.BitwiseSingleEqualityComparer.Equals(Shg6, other.Shg6)) return false;
      if (!pbc::ProtobufEqualityComparers.BitwiseSingleEqualityComparer.Equals(Shg7, other.Shg7)) return false;
      if (!pbc::ProtobufEqualityComparers.BitwiseSingleEqualityComparer.Equals(Shg8, other.Shg8)) return false;
      if (!pbc::ProtobufEqualityComparers.BitwiseSingleEqualityComparer.Equals(Shb0, other.Shb0)) return false;
      if (!pbc::ProtobufEqualityComparers.BitwiseSingleEqualityComparer.Equals(Shb1, other.Shb1)) return false;
      if (!pbc::ProtobufEqualityComparers.BitwiseSingleEqualityComparer.Equals(Shb2, other.Shb2)) return false;
      if (!pbc::ProtobufEqualityComparers.BitwiseSingleEqualityComparer.Equals(Shb3, other.Shb3)) return false;
      if (!pbc::ProtobufEqualityComparers.BitwiseSingleEqualityComparer.Equals(Shb4, other.Shb4)) return false;
      if (!pbc::ProtobufEqualityComparers.BitwiseSingleEqualityComparer.Equals(Shb5, other.Shb5)) return false;
      if (!pbc::ProtobufEqualityComparers.BitwiseSingleEqualityComparer.Equals(Shb6, other.Shb6)) return false;
      if (!pbc::ProtobufEqualityComparers.BitwiseSingleEqualityComparer.Equals(Shb7, other.Shb7)) return false;
      if (!pbc::ProtobufEqualityComparers.BitwiseSingleEqualityComparer.Equals(Shb8, other.Shb8)) return false;
      return Equals(_unknownFields, other._unknownFields);
    }

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    [global::System.CodeDom.Compiler.GeneratedCode("protoc", null)]
    public override int GetHashCode() {
      int hash = 1;
      if (Shr0 != 0F) hash ^= pbc::ProtobufEqualityComparers.BitwiseSingleEqualityComparer.GetHashCode(Shr0);
      if (Shr1 != 0F) hash ^= pbc::ProtobufEqualityComparers.BitwiseSingleEqualityComparer.GetHashCode(Shr1);
      if (Shr2 != 0F) hash ^= pbc::ProtobufEqualityComparers.BitwiseSingleEqualityComparer.GetHashCode(Shr2);
      if (Shr3 != 0F) hash ^= pbc::ProtobufEqualityComparers.BitwiseSingleEqualityComparer.GetHashCode(Shr3);
      if (Shr4 != 0F) hash ^= pbc::ProtobufEqualityComparers.BitwiseSingleEqualityComparer.GetHashCode(Shr4);
      if (Shr5 != 0F) hash ^= pbc::ProtobufEqualityComparers.BitwiseSingleEqualityComparer.GetHashCode(Shr5);
      if (Shr6 != 0F) hash ^= pbc::ProtobufEqualityComparers.BitwiseSingleEqualityComparer.GetHashCode(Shr6);
      if (Shr7 != 0F) hash ^= pbc::ProtobufEqualityComparers.BitwiseSingleEqualityComparer.GetHashCode(Shr7);
      if (Shr8 != 0F) hash ^= pbc::ProtobufEqualityComparers.BitwiseSingleEqualityComparer.GetHashCode(Shr8);
      if (Shg0 != 0F) hash ^= pbc::ProtobufEqualityComparers.BitwiseSingleEqualityComparer.GetHashCode(Shg0);
      if (Shg1 != 0F) hash ^= pbc::ProtobufEqualityComparers.BitwiseSingleEqualityComparer.GetHashCode(Shg1);
      if (Shg2 != 0F) hash ^= pbc::ProtobufEqualityComparers.BitwiseSingleEqualityComparer.GetHashCode(Shg2);
      if (Shg3 != 0F) hash ^= pbc::ProtobufEqualityComparers.BitwiseSingleEqualityComparer.GetHashCode(Shg3);
      if (Shg4 != 0F) hash ^= pbc::ProtobufEqualityComparers.BitwiseSingleEqualityComparer.GetHashCode(Shg4);
      if (Shg5 != 0F) hash ^= pbc::ProtobufEqualityComparers.BitwiseSingleEqualityComparer.GetHashCode(Shg5);
      if (Shg6 != 0F) hash ^= pbc::ProtobufEqualityComparers.BitwiseSingleEqualityComparer.GetHashCode(Shg6);
      if (Shg7 != 0F) hash ^= pbc::ProtobufEqualityComparers.BitwiseSingleEqualityComparer.GetHashCode(Shg7);
      if (Shg8 != 0F) hash ^= pbc::ProtobufEqualityComparers.BitwiseSingleEqualityComparer.GetHashCode(Shg8);
      if (Shb0 != 0F) hash ^= pbc::ProtobufEqualityComparers.BitwiseSingleEqualityComparer.GetHashCode(Shb0);
      if (Shb1 != 0F) hash ^= pbc::ProtobufEqualityComparers.BitwiseSingleEqualityComparer.GetHashCode(Shb1);
      if (Shb2 != 0F) hash ^= pbc::ProtobufEqualityComparers.BitwiseSingleEqualityComparer.GetHashCode(Shb2);
      if (Shb3 != 0F) hash ^= pbc::ProtobufEqualityComparers.BitwiseSingleEqualityComparer.GetHashCode(Shb3);
      if (Shb4 != 0F) hash ^= pbc::ProtobufEqualityComparers.BitwiseSingleEqualityComparer.GetHashCode(Shb4);
      if (Shb5 != 0F) hash ^= pbc::ProtobufEqualityComparers.BitwiseSingleEqualityComparer.GetHashCode(Shb5);
      if (Shb6 != 0F) hash ^= pbc::ProtobufEqualityComparers.BitwiseSingleEqualityComparer.GetHashCode(Shb6);
      if (Shb7 != 0F) hash ^= pbc::ProtobufEqualityComparers.BitwiseSingleEqualityComparer.GetHashCode(Shb7);
      if (Shb8 != 0F) hash ^= pbc::ProtobufEqualityComparers.BitwiseSingleEqualityComparer.GetHashCode(Shb8);
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
      if (Shr0 != 0F) {
        output.WriteRawTag(13);
        output.WriteFloat(Shr0);
      }
      if (Shr1 != 0F) {
        output.WriteRawTag(21);
        output.WriteFloat(Shr1);
      }
      if (Shr2 != 0F) {
        output.WriteRawTag(29);
        output.WriteFloat(Shr2);
      }
      if (Shr3 != 0F) {
        output.WriteRawTag(37);
        output.WriteFloat(Shr3);
      }
      if (Shr4 != 0F) {
        output.WriteRawTag(45);
        output.WriteFloat(Shr4);
      }
      if (Shr5 != 0F) {
        output.WriteRawTag(53);
        output.WriteFloat(Shr5);
      }
      if (Shr6 != 0F) {
        output.WriteRawTag(61);
        output.WriteFloat(Shr6);
      }
      if (Shr7 != 0F) {
        output.WriteRawTag(69);
        output.WriteFloat(Shr7);
      }
      if (Shr8 != 0F) {
        output.WriteRawTag(77);
        output.WriteFloat(Shr8);
      }
      if (Shg0 != 0F) {
        output.WriteRawTag(85);
        output.WriteFloat(Shg0);
      }
      if (Shg1 != 0F) {
        output.WriteRawTag(93);
        output.WriteFloat(Shg1);
      }
      if (Shg2 != 0F) {
        output.WriteRawTag(101);
        output.WriteFloat(Shg2);
      }
      if (Shg3 != 0F) {
        output.WriteRawTag(109);
        output.WriteFloat(Shg3);
      }
      if (Shg4 != 0F) {
        output.WriteRawTag(117);
        output.WriteFloat(Shg4);
      }
      if (Shg5 != 0F) {
        output.WriteRawTag(125);
        output.WriteFloat(Shg5);
      }
      if (Shg6 != 0F) {
        output.WriteRawTag(133, 1);
        output.WriteFloat(Shg6);
      }
      if (Shg7 != 0F) {
        output.WriteRawTag(141, 1);
        output.WriteFloat(Shg7);
      }
      if (Shg8 != 0F) {
        output.WriteRawTag(149, 1);
        output.WriteFloat(Shg8);
      }
      if (Shb0 != 0F) {
        output.WriteRawTag(157, 1);
        output.WriteFloat(Shb0);
      }
      if (Shb1 != 0F) {
        output.WriteRawTag(165, 1);
        output.WriteFloat(Shb1);
      }
      if (Shb2 != 0F) {
        output.WriteRawTag(173, 1);
        output.WriteFloat(Shb2);
      }
      if (Shb3 != 0F) {
        output.WriteRawTag(181, 1);
        output.WriteFloat(Shb3);
      }
      if (Shb4 != 0F) {
        output.WriteRawTag(189, 1);
        output.WriteFloat(Shb4);
      }
      if (Shb5 != 0F) {
        output.WriteRawTag(197, 1);
        output.WriteFloat(Shb5);
      }
      if (Shb6 != 0F) {
        output.WriteRawTag(205, 1);
        output.WriteFloat(Shb6);
      }
      if (Shb7 != 0F) {
        output.WriteRawTag(213, 1);
        output.WriteFloat(Shb7);
      }
      if (Shb8 != 0F) {
        output.WriteRawTag(221, 1);
        output.WriteFloat(Shb8);
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
      if (Shr0 != 0F) {
        output.WriteRawTag(13);
        output.WriteFloat(Shr0);
      }
      if (Shr1 != 0F) {
        output.WriteRawTag(21);
        output.WriteFloat(Shr1);
      }
      if (Shr2 != 0F) {
        output.WriteRawTag(29);
        output.WriteFloat(Shr2);
      }
      if (Shr3 != 0F) {
        output.WriteRawTag(37);
        output.WriteFloat(Shr3);
      }
      if (Shr4 != 0F) {
        output.WriteRawTag(45);
        output.WriteFloat(Shr4);
      }
      if (Shr5 != 0F) {
        output.WriteRawTag(53);
        output.WriteFloat(Shr5);
      }
      if (Shr6 != 0F) {
        output.WriteRawTag(61);
        output.WriteFloat(Shr6);
      }
      if (Shr7 != 0F) {
        output.WriteRawTag(69);
        output.WriteFloat(Shr7);
      }
      if (Shr8 != 0F) {
        output.WriteRawTag(77);
        output.WriteFloat(Shr8);
      }
      if (Shg0 != 0F) {
        output.WriteRawTag(85);
        output.WriteFloat(Shg0);
      }
      if (Shg1 != 0F) {
        output.WriteRawTag(93);
        output.WriteFloat(Shg1);
      }
      if (Shg2 != 0F) {
        output.WriteRawTag(101);
        output.WriteFloat(Shg2);
      }
      if (Shg3 != 0F) {
        output.WriteRawTag(109);
        output.WriteFloat(Shg3);
      }
      if (Shg4 != 0F) {
        output.WriteRawTag(117);
        output.WriteFloat(Shg4);
      }
      if (Shg5 != 0F) {
        output.WriteRawTag(125);
        output.WriteFloat(Shg5);
      }
      if (Shg6 != 0F) {
        output.WriteRawTag(133, 1);
        output.WriteFloat(Shg6);
      }
      if (Shg7 != 0F) {
        output.WriteRawTag(141, 1);
        output.WriteFloat(Shg7);
      }
      if (Shg8 != 0F) {
        output.WriteRawTag(149, 1);
        output.WriteFloat(Shg8);
      }
      if (Shb0 != 0F) {
        output.WriteRawTag(157, 1);
        output.WriteFloat(Shb0);
      }
      if (Shb1 != 0F) {
        output.WriteRawTag(165, 1);
        output.WriteFloat(Shb1);
      }
      if (Shb2 != 0F) {
        output.WriteRawTag(173, 1);
        output.WriteFloat(Shb2);
      }
      if (Shb3 != 0F) {
        output.WriteRawTag(181, 1);
        output.WriteFloat(Shb3);
      }
      if (Shb4 != 0F) {
        output.WriteRawTag(189, 1);
        output.WriteFloat(Shb4);
      }
      if (Shb5 != 0F) {
        output.WriteRawTag(197, 1);
        output.WriteFloat(Shb5);
      }
      if (Shb6 != 0F) {
        output.WriteRawTag(205, 1);
        output.WriteFloat(Shb6);
      }
      if (Shb7 != 0F) {
        output.WriteRawTag(213, 1);
        output.WriteFloat(Shb7);
      }
      if (Shb8 != 0F) {
        output.WriteRawTag(221, 1);
        output.WriteFloat(Shb8);
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
      if (Shr0 != 0F) {
        size += 1 + 4;
      }
      if (Shr1 != 0F) {
        size += 1 + 4;
      }
      if (Shr2 != 0F) {
        size += 1 + 4;
      }
      if (Shr3 != 0F) {
        size += 1 + 4;
      }
      if (Shr4 != 0F) {
        size += 1 + 4;
      }
      if (Shr5 != 0F) {
        size += 1 + 4;
      }
      if (Shr6 != 0F) {
        size += 1 + 4;
      }
      if (Shr7 != 0F) {
        size += 1 + 4;
      }
      if (Shr8 != 0F) {
        size += 1 + 4;
      }
      if (Shg0 != 0F) {
        size += 1 + 4;
      }
      if (Shg1 != 0F) {
        size += 1 + 4;
      }
      if (Shg2 != 0F) {
        size += 1 + 4;
      }
      if (Shg3 != 0F) {
        size += 1 + 4;
      }
      if (Shg4 != 0F) {
        size += 1 + 4;
      }
      if (Shg5 != 0F) {
        size += 1 + 4;
      }
      if (Shg6 != 0F) {
        size += 2 + 4;
      }
      if (Shg7 != 0F) {
        size += 2 + 4;
      }
      if (Shg8 != 0F) {
        size += 2 + 4;
      }
      if (Shb0 != 0F) {
        size += 2 + 4;
      }
      if (Shb1 != 0F) {
        size += 2 + 4;
      }
      if (Shb2 != 0F) {
        size += 2 + 4;
      }
      if (Shb3 != 0F) {
        size += 2 + 4;
      }
      if (Shb4 != 0F) {
        size += 2 + 4;
      }
      if (Shb5 != 0F) {
        size += 2 + 4;
      }
      if (Shb6 != 0F) {
        size += 2 + 4;
      }
      if (Shb7 != 0F) {
        size += 2 + 4;
      }
      if (Shb8 != 0F) {
        size += 2 + 4;
      }
      if (_unknownFields != null) {
        size += _unknownFields.CalculateSize();
      }
      return size;
    }

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    [global::System.CodeDom.Compiler.GeneratedCode("protoc", null)]
    public void MergeFrom(SphericalHarmonicsL2 other) {
      if (other == null) {
        return;
      }
      if (other.Shr0 != 0F) {
        Shr0 = other.Shr0;
      }
      if (other.Shr1 != 0F) {
        Shr1 = other.Shr1;
      }
      if (other.Shr2 != 0F) {
        Shr2 = other.Shr2;
      }
      if (other.Shr3 != 0F) {
        Shr3 = other.Shr3;
      }
      if (other.Shr4 != 0F) {
        Shr4 = other.Shr4;
      }
      if (other.Shr5 != 0F) {
        Shr5 = other.Shr5;
      }
      if (other.Shr6 != 0F) {
        Shr6 = other.Shr6;
      }
      if (other.Shr7 != 0F) {
        Shr7 = other.Shr7;
      }
      if (other.Shr8 != 0F) {
        Shr8 = other.Shr8;
      }
      if (other.Shg0 != 0F) {
        Shg0 = other.Shg0;
      }
      if (other.Shg1 != 0F) {
        Shg1 = other.Shg1;
      }
      if (other.Shg2 != 0F) {
        Shg2 = other.Shg2;
      }
      if (other.Shg3 != 0F) {
        Shg3 = other.Shg3;
      }
      if (other.Shg4 != 0F) {
        Shg4 = other.Shg4;
      }
      if (other.Shg5 != 0F) {
        Shg5 = other.Shg5;
      }
      if (other.Shg6 != 0F) {
        Shg6 = other.Shg6;
      }
      if (other.Shg7 != 0F) {
        Shg7 = other.Shg7;
      }
      if (other.Shg8 != 0F) {
        Shg8 = other.Shg8;
      }
      if (other.Shb0 != 0F) {
        Shb0 = other.Shb0;
      }
      if (other.Shb1 != 0F) {
        Shb1 = other.Shb1;
      }
      if (other.Shb2 != 0F) {
        Shb2 = other.Shb2;
      }
      if (other.Shb3 != 0F) {
        Shb3 = other.Shb3;
      }
      if (other.Shb4 != 0F) {
        Shb4 = other.Shb4;
      }
      if (other.Shb5 != 0F) {
        Shb5 = other.Shb5;
      }
      if (other.Shb6 != 0F) {
        Shb6 = other.Shb6;
      }
      if (other.Shb7 != 0F) {
        Shb7 = other.Shb7;
      }
      if (other.Shb8 != 0F) {
        Shb8 = other.Shb8;
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
          case 13: {
            Shr0 = input.ReadFloat();
            break;
          }
          case 21: {
            Shr1 = input.ReadFloat();
            break;
          }
          case 29: {
            Shr2 = input.ReadFloat();
            break;
          }
          case 37: {
            Shr3 = input.ReadFloat();
            break;
          }
          case 45: {
            Shr4 = input.ReadFloat();
            break;
          }
          case 53: {
            Shr5 = input.ReadFloat();
            break;
          }
          case 61: {
            Shr6 = input.ReadFloat();
            break;
          }
          case 69: {
            Shr7 = input.ReadFloat();
            break;
          }
          case 77: {
            Shr8 = input.ReadFloat();
            break;
          }
          case 85: {
            Shg0 = input.ReadFloat();
            break;
          }
          case 93: {
            Shg1 = input.ReadFloat();
            break;
          }
          case 101: {
            Shg2 = input.ReadFloat();
            break;
          }
          case 109: {
            Shg3 = input.ReadFloat();
            break;
          }
          case 117: {
            Shg4 = input.ReadFloat();
            break;
          }
          case 125: {
            Shg5 = input.ReadFloat();
            break;
          }
          case 133: {
            Shg6 = input.ReadFloat();
            break;
          }
          case 141: {
            Shg7 = input.ReadFloat();
            break;
          }
          case 149: {
            Shg8 = input.ReadFloat();
            break;
          }
          case 157: {
            Shb0 = input.ReadFloat();
            break;
          }
          case 165: {
            Shb1 = input.ReadFloat();
            break;
          }
          case 173: {
            Shb2 = input.ReadFloat();
            break;
          }
          case 181: {
            Shb3 = input.ReadFloat();
            break;
          }
          case 189: {
            Shb4 = input.ReadFloat();
            break;
          }
          case 197: {
            Shb5 = input.ReadFloat();
            break;
          }
          case 205: {
            Shb6 = input.ReadFloat();
            break;
          }
          case 213: {
            Shb7 = input.ReadFloat();
            break;
          }
          case 221: {
            Shb8 = input.ReadFloat();
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
          case 13: {
            Shr0 = input.ReadFloat();
            break;
          }
          case 21: {
            Shr1 = input.ReadFloat();
            break;
          }
          case 29: {
            Shr2 = input.ReadFloat();
            break;
          }
          case 37: {
            Shr3 = input.ReadFloat();
            break;
          }
          case 45: {
            Shr4 = input.ReadFloat();
            break;
          }
          case 53: {
            Shr5 = input.ReadFloat();
            break;
          }
          case 61: {
            Shr6 = input.ReadFloat();
            break;
          }
          case 69: {
            Shr7 = input.ReadFloat();
            break;
          }
          case 77: {
            Shr8 = input.ReadFloat();
            break;
          }
          case 85: {
            Shg0 = input.ReadFloat();
            break;
          }
          case 93: {
            Shg1 = input.ReadFloat();
            break;
          }
          case 101: {
            Shg2 = input.ReadFloat();
            break;
          }
          case 109: {
            Shg3 = input.ReadFloat();
            break;
          }
          case 117: {
            Shg4 = input.ReadFloat();
            break;
          }
          case 125: {
            Shg5 = input.ReadFloat();
            break;
          }
          case 133: {
            Shg6 = input.ReadFloat();
            break;
          }
          case 141: {
            Shg7 = input.ReadFloat();
            break;
          }
          case 149: {
            Shg8 = input.ReadFloat();
            break;
          }
          case 157: {
            Shb0 = input.ReadFloat();
            break;
          }
          case 165: {
            Shb1 = input.ReadFloat();
            break;
          }
          case 173: {
            Shb2 = input.ReadFloat();
            break;
          }
          case 181: {
            Shb3 = input.ReadFloat();
            break;
          }
          case 189: {
            Shb4 = input.ReadFloat();
            break;
          }
          case 197: {
            Shb5 = input.ReadFloat();
            break;
          }
          case 205: {
            Shb6 = input.ReadFloat();
            break;
          }
          case 213: {
            Shb7 = input.ReadFloat();
            break;
          }
          case 221: {
            Shb8 = input.ReadFloat();
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
