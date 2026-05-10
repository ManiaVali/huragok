using System.Numerics;
using Huragok.Data.IntermediateFormats;
using Huragok.Data.IntermediateFormats.Color;
using Huragok.Data.IntermediateFormats.Coordinates;

namespace Huragok.Utilities.Serializer {
    /// <summary>
    /// <para>Class used to read an entire tag and prepare it for serialization via <see cref="DataSerializer"/>.</para>
    /// <para>Should not be used when constructing tags for export, as it processes the entire tag when we rarely need that.</para>
    /// </summary>
    internal static class TagSerializer {
        // Do not bother parsing these types of fields. (Yet)
        private static readonly List<TagFieldType> skipTypes = [
            TagFieldType.Explanation,
        ];

        private static IEnumerable<string>? skipFields;

        internal static object ReadTag(TagFile tagFile, IEnumerable<string>? skipFieldNames = null) {
            skipFields = skipFieldNames?.ToHashSet();
            return ReadFields(tagFile.Fields);
        }

        private static Dictionary<string, object?> ReadFields(IEnumerable<TagField> fields) {
            var result = new Dictionary<string, object?>();

            foreach (var field in fields) {
                if (skipTypes.Contains(field.FieldType)) continue;
                if (skipFields is not null && skipFields.Contains(field.FieldName)) continue;
                if (field.Visible == false) continue;

                if (field.FieldType is TagFieldType.Block) {
                    if (((TagFieldBlock)field).Elements.Count == 0) continue;
                }
                result[field.FieldName] = ReadField(field);
            }

            return result;
        }

        private static List<object> ReadBlock(TagFieldBlock block) {
            var list = new List<object>();

            foreach (var element in block.Elements) {
                list.Add(ReadFields(element.Fields));
            }

            return list;
        }

        private static List<object> ReadStruct(TagFieldStruct @struct) {
            var list = new List<object>();

            foreach (var element in @struct.Elements) {
                list.Add(ReadFields(element.Fields));
            }

            return list;
        }

        private static Dictionary<string, bool> ReadFlags(TagFieldFlags flags) => flags.Items.ToDictionary(k => k.FlagName, v => flags.TestBit(v.FlagName));
        private static Dictionary<string, bool> ReadBlockFlags(TagFieldBlockFlags flags) => flags.Items.ToDictionary(k => k.FlagName, v => v.IsSet);

        // TODO: Convert these to truly use RealPoints so we can autoconvert their coordinate spaces later.
        private static Vector2 ReadPoint2d(TagFieldElementArrayInteger integerArray) => new(integerArray.Data[0], integerArray.Data[1]);
        private static Vector2 ReadPoint2d(TagFieldElementArraySingle floatArray) => new(floatArray.Data[0], floatArray.Data[1]);
        private static Vector3 ReadPoint3d(TagFieldElementArraySingle floatArray) => new(floatArray.Data[0], floatArray.Data[1], floatArray.Data[2]);
        private static RealPlane2d ReadPlane2d(TagFieldElementArraySingle floatArray) => new(floatArray.Data[0], floatArray.Data[1], floatArray.Data[2]);
        private static RealPlane3d ReadPlane3d(TagFieldElementArraySingle floatArray) => new(floatArray.Data[0], floatArray.Data[1], floatArray.Data[2], floatArray.Data[3]);
        private static float ReadAngle(TagFieldElementSingle angle) => angle.Data;
        private static Quaternion ReadQuaternion(TagFieldElementArraySingle floatArray) => new(floatArray.Data[0], floatArray.Data[1], floatArray.Data[2], floatArray.Data[3]);
        private static IF_Color ReadColorRGBA(TagFieldElementArraySingle floatArray) {
            int? alpha = floatArray.Count == 4 ? FloatToColorInt(floatArray.Data[0]) : null;
            var red = floatArray.Count == 4 ? FloatToColorInt(floatArray.Data[1]) : FloatToColorInt(floatArray.Data[0]);
            var green = floatArray.Count == 4 ? FloatToColorInt(floatArray.Data[2]) : FloatToColorInt(floatArray.Data[1]);
            var blue = floatArray.Count == 4 ? FloatToColorInt(floatArray.Data[3]) : FloatToColorInt(floatArray.Data[2]);

            return new(red, green, blue, alpha, IF_ColorMode.Xbox);

            int FloatToColorInt(float floatValue) {
                return Convert.ToInt32(floatValue * 255);
            }
        }
        private static object ReadArray(TagFieldArray array) {
            var list = new List<object>();

            foreach (var element in array.Elements.Cast<TagFieldArrayElement>()) {
                if (element.Fields != null && element.Fields.Length > 0) {
                    list.Add(ReadFields(element.Fields));
                }
            }

            return list;
        }
        // END

        private static object ReadCustom(TagFieldCustom customElement) {
            if (customElement.GetType() == typeof(TagFieldCustomFunctionEditor)) {
                var func = (TagFieldCustomFunctionEditor)customElement;
                var editor = func.Value;

                if (editor.MasterType != FunctionEditorMasterType.Exponent || editor.MasterType != FunctionEditorMasterType.Periodic) {
                    return $"unsupported function type: {editor.MasterType} ({customElement.GetType().Name})";
                }

                return new IF_Function((TagFieldCustomFunctionEditor)customElement);
            } else {
#if DEBUG
                return $"unsupported custom type: {customElement.CustomType} ({customElement.GetType().Name})";
#else
                return null;
#endif
            }
        }

        private static object UnsupportedType(TagField field) {
#if DEBUG
            return $"field not readable, unsupported field type: {field.FieldType} ({field.GetType().Name})";
#else
            return null;
#endif
        }

        private static object? ReadField(TagField field) {
            return field.FieldType switch {
                TagFieldType.String => ((TagFieldElementString)field).Data,
                TagFieldType.LongString => ((TagFieldElementLongString)field).Data,
                TagFieldType.StringId => ((TagFieldElementStringID)field).Data,
                TagFieldType.OldStringId => ((TagFieldElementOldStringID)field).Data,
                TagFieldType.CharInteger => ((TagFieldElementInteger)field).Data,
                TagFieldType.ShortInteger => ((TagFieldElementInteger)field).Data,
                TagFieldType.LongInteger => ((TagFieldElementInteger)field).Data,
                TagFieldType.Int64Integer => ((TagFieldElementInteger)field).Data,
                TagFieldType.Angle => ReadAngle((TagFieldElementSingle)field),
                TagFieldType.Tag => ((TagFieldElementTag)field).File.Path.RelativePathWithExtension, // Not sure what this is for
                TagFieldType.CharEnum => ((TagFieldEnum)field).Value,
                TagFieldType.ShortEnum => ((TagFieldEnum)field).Value,
                TagFieldType.LongEnum => ((TagFieldEnum)field).Value,
                TagFieldType.Flags => ReadFlags((TagFieldFlags)field),
                TagFieldType.WordFlags => ReadFlags((TagFieldFlags)field),
                TagFieldType.ByteFlags => ReadFlags((TagFieldFlags)field),
                TagFieldType.Point2d => ReadPoint2d((TagFieldElementArrayInteger)field),
                TagFieldType.Rectangle2d => UnsupportedType(field), // Not supported
                TagFieldType.RgbPixel32 => UnsupportedType(field), // Not supported
                TagFieldType.ArgbPixel32 => UnsupportedType(field), // Not supported
                TagFieldType.Real => ((TagFieldElementSingle)field).Data,
                TagFieldType.RealSlider => ((TagFieldElementSingle)field).Data,
                TagFieldType.RealFraction => ((TagFieldElementSingle)field).Data,
                TagFieldType.RealPoint2d => ReadPoint2d((TagFieldElementArraySingle)field),
                TagFieldType.RealPoint3d => ReadPoint3d((TagFieldElementArraySingle)field),
                TagFieldType.RealVector2d => ReadPoint2d((TagFieldElementArraySingle)field),
                TagFieldType.RealVector3d => ReadPoint3d((TagFieldElementArraySingle)field),
                TagFieldType.RealQuaternion => ReadQuaternion((TagFieldElementArraySingle)field),
                TagFieldType.RealEulerAngles2d => ReadPoint2d((TagFieldElementArraySingle)field),
                TagFieldType.RealEulerAngles3d => ReadPoint3d((TagFieldElementArraySingle)field),
                TagFieldType.RealPlane2d => ReadPlane2d((TagFieldElementArraySingle)field),
                TagFieldType.RealPlane3d => ReadPlane3d((TagFieldElementArraySingle)field),
                TagFieldType.RealRgbColor => ReadColorRGBA((TagFieldElementArraySingle)field),
                TagFieldType.RealArgbColor => ReadColorRGBA((TagFieldElementArraySingle)field),
                TagFieldType.RealHsvColor => UnsupportedType(field), // Not supported
                TagFieldType.RealAhsvColor => UnsupportedType(field), // Not supported
                TagFieldType.ShortIntegerBounds => UnsupportedType(field), // Not supported
                TagFieldType.AngleBounds => UnsupportedType(field), // Not supported
                TagFieldType.RealBounds => UnsupportedType(field), // Not supported
                TagFieldType.RealFractionBounds => UnsupportedType(field), // Not supported
                TagFieldType.Reference => ((TagFieldReference)field).Reference?.Path?.RelativePathWithExtension,
                TagFieldType.Block => ReadBlock((TagFieldBlock)field),
                TagFieldType.BlockFlags => ReadBlockFlags((TagFieldBlockFlags)field),
                TagFieldType.WordBlockFlags => ReadBlockFlags((TagFieldBlockFlags)field),
                TagFieldType.ByteBlockFlags => ReadBlockFlags((TagFieldBlockFlags)field),
                TagFieldType.CharBlockIndex => UnsupportedType(field), // Not supported
                TagFieldType.CharBlockIndexCustomSearch => UnsupportedType(field), // Not supported
                TagFieldType.ShortBlockIndex => ((TagFieldBlockIndex)field).Value,
                TagFieldType.ShortBlockIndexCustomSearch => UnsupportedType(field), // Not supported
                TagFieldType.LongBlockIndex => UnsupportedType(field), // Not supported
                TagFieldType.LongBlockIndexCustomSearch => UnsupportedType(field), // Not supported
                TagFieldType.Data => "binary data",
                TagFieldType.VertexBuffer => UnsupportedType(field), // Not supported
                TagFieldType.Pad => UnsupportedType(field), // Not supported
                TagFieldType.UselessPad => UnsupportedType(field), // Not supported
                TagFieldType.Skip => UnsupportedType(field), // Not supported
                TagFieldType.Explanation => UnsupportedType(field), // Not supported
                TagFieldType.Custom => ReadCustom((TagFieldCustom)field),
                TagFieldType.Struct => ReadStruct((TagFieldStruct)field),
                TagFieldType.Array => ReadArray((TagFieldArray)field),
                TagFieldType.Resource => UnsupportedType(field), // Not supported
                TagFieldType.Interop => UnsupportedType(field), // Not supported
                TagFieldType.Terminator => UnsupportedType(field), // Not supported
                _ => throw new NotImplementedException(),
            };
        }
    }
}