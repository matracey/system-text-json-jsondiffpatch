﻿using System.Text.Json.JsonDiffPatch.Diffs;
using System.Text.Json.Nodes;

namespace System.Text.Json.JsonDiffPatch
{
    static partial class JsonDiffPatcher
    {
        /// <summary>
        /// Applies changes in the patch document to the JSON object.
        /// </summary>
        /// <param name="left">The JSON object to which patch is applied.</param>
        /// <param name="patch">The patch document previously generated by <c>Diff</c> method.</param>
        /// <param name="options">The patch options.</param>
        public static void Patch(ref JsonNode? left, JsonNode? patch, JsonPatchOptions options = default)
        {
            // When make changes in this method, also copy the changes to ReversePatch method

            if (patch is null)
            {
                return;
            }

            var delta = new JsonDiffDelta(patch);
            var kind = delta.Kind;

            switch (kind)
            {
                case DeltaKind.Modified:
                    left = delta.GetNewValue();
                    return;
                case DeltaKind.Text
                    when left is JsonValue jsonValue
                         && jsonValue.TryGetValue<string>(out var text):
                    left = PatchLongText(text, delta, options);
                    return;
                case DeltaKind.Object when left is JsonObject jsonObj:
                    PatchObject(jsonObj, patch.AsObject(), options);
                    return;
                case DeltaKind.Array when left is JsonArray jsonArray:
                    PatchArray(jsonArray, patch.AsObject(), options);
                    return;
                default:
                    throw new FormatException(JsonDiffDelta.InvalidPatchDocument);
            }
        }

        /// <summary>
        /// Creates a deep copy the JSON object and applies changes in the patch document to the copy.
        /// </summary>
        /// <param name="left">The JSON object.</param>
        /// <param name="patch">The patch document previously generated by <c>Diff</c> method.</param>
        /// <param name="options">The patch options.</param>
        public static JsonNode? PatchNew(this JsonNode? left, JsonNode? patch, JsonPatchOptions options = default)
        {
            var copy = left?.DeepClone();
            Patch(ref copy, patch, options);
            return copy;
        }

        /// <summary>
        /// Reverses the changes made by a previous call to <see cref="Patch"/>.
        /// </summary>
        /// <param name="right">The JSON object from which patch is reversed.</param>
        /// <param name="patch">The patch document previously generated by <c>Diff</c> method.</param>
        /// <param name="options">The patch options.</param>
        public static void ReversePatch(ref JsonNode? right, JsonNode? patch, JsonReversePatchOptions options = default)
        {
            // When make changes in this method, also copy the changes to Patch method

            if (patch is null)
            {
                return;
            }

            var delta = new JsonDiffDelta(patch);
            var kind = delta.Kind;

            switch (kind)
            {
                case DeltaKind.Modified:
                    right = delta.GetOldValue();
                    return;
                case DeltaKind.Text
                    when right is JsonValue jsonValue
                         && jsonValue.TryGetValue<string>(out var text):
                    right = ReversePatchLongText(text, delta, options);
                    return;
                case DeltaKind.Object when right is JsonObject jsonObj:
                    ReversePatchObject(jsonObj, patch.AsObject(), options);
                    return;
                case DeltaKind.Array when right is JsonArray jsonArray:
                    ReversePatchArray(jsonArray, patch.AsObject(), options);
                    return;
                default:
                    throw new FormatException(JsonDiffDelta.InvalidPatchDocument);
            }
        }

        /// <summary>
        /// Creates a deep copy the JSON object and reverses the changes made by
        /// a previous call to <see cref="Patch"/> from the copy.
        /// </summary>
        /// <param name="right">The JSON object.</param>
        /// <param name="patch">The patch document previously generated by <c>Diff</c> method.</param>
        /// <param name="options">The patch options.</param>
        public static JsonNode? ReversePatchNew(this JsonNode? right, JsonNode? patch, JsonReversePatchOptions options = default)
        {
            var copy = right?.DeepClone();
            ReversePatch(ref copy, patch, options);
            return copy;
        }
    }
}