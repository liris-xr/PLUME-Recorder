// <auto-generated>
//     Generated by the protocol buffer compiler.  DO NOT EDIT!
//     source: plume/sample/unity/urp/rendering.proto
// </auto-generated>
#pragma warning disable 1591, 0612, 3021, 8981
#region Designer generated code

using pb = global::Google.Protobuf;
using pbc = global::Google.Protobuf.Collections;
using pbr = global::Google.Protobuf.Reflection;
using scg = global::System.Collections.Generic;
namespace PLUME.Sample.Unity.URP {

  /// <summary>Holder for reflection information generated from plume/sample/unity/urp/rendering.proto</summary>
  public static partial class RenderingReflection {

    #region Descriptor
    /// <summary>File descriptor for plume/sample/unity/urp/rendering.proto</summary>
    public static pbr::FileDescriptor Descriptor {
      get { return descriptor; }
    }
    private static pbr::FileDescriptor descriptor;

    static RenderingReflection() {
      byte[] descriptorData = global::System.Convert.FromBase64String(
          string.Concat(
            "CiZwbHVtZS9zYW1wbGUvdW5pdHkvdXJwL3JlbmRlcmluZy5wcm90bxIWcGx1",
            "bWUuc2FtcGxlLnVuaXR5LnVycCqfAQoQQW50aWFsaWFzaW5nTW9kZRIaChZB",
            "TlRJQUxJQVNJTkdfTU9ERV9OT05FEAASMwovQU5USUFMSUFTSU5HX01PREVf",
            "RkFTVF9BUFBST1hJTUFURV9BTlRJQUxJQVNJTkcQARI6CjZBTlRJQUxJQVNJ",
            "TkdfTU9ERV9TVUJQSVhFTF9NT1JQSE9MT0dJQ0FMX0FOVElfQUxJQVNJTkcQ",
            "AipzChNBbnRpYWxpYXNpbmdRdWFsaXR5EhwKGEFOVElBTElBU0lOR19RVUFM",
            "SVRZX0xPVxAAEh8KG0FOVElBTElBU0lOR19RVUFMSVRZX01FRElVTRABEh0K",
            "GUFOVElBTElBU0lOR19RVUFMSVRZX0hJR0gQAkIZqgIWUExVTUUuU2FtcGxl",
            "LlVuaXR5LlVSUGIGcHJvdG8z"));
      descriptor = pbr::FileDescriptor.FromGeneratedCode(descriptorData,
          new pbr::FileDescriptor[] { },
          new pbr::GeneratedClrTypeInfo(new[] {typeof(global::PLUME.Sample.Unity.URP.AntialiasingMode), typeof(global::PLUME.Sample.Unity.URP.AntialiasingQuality), }, null, null));
    }
    #endregion

  }
  #region Enums
  public enum AntialiasingMode {
    [pbr::OriginalName("ANTIALIASING_MODE_NONE")] None = 0,
    [pbr::OriginalName("ANTIALIASING_MODE_FAST_APPROXIMATE_ANTIALIASING")] FastApproximateAntialiasing = 1,
    [pbr::OriginalName("ANTIALIASING_MODE_SUBPIXEL_MORPHOLOGICAL_ANTI_ALIASING")] SubpixelMorphologicalAntiAliasing = 2,
  }

  public enum AntialiasingQuality {
    [pbr::OriginalName("ANTIALIASING_QUALITY_LOW")] Low = 0,
    [pbr::OriginalName("ANTIALIASING_QUALITY_MEDIUM")] Medium = 1,
    [pbr::OriginalName("ANTIALIASING_QUALITY_HIGH")] High = 2,
  }

  #endregion

}

#endregion Designer generated code
