﻿// Copyright 2013-2020 Dirk Lemstra <https://github.com/dlemstra/Magick.NET/>
//
// Licensed under the ImageMagick License (the "License"); you may not use this file except in
// compliance with the License. You may obtain a copy of the License at
//
//   https://www.imagemagick.org/script/license.php
//
// Unless required by applicable law or agreed to in writing, software distributed under the
// License is distributed on an "AS IS" BASIS, WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND,
// either express or implied. See the License for the specific language governing permissions
// and limitations under the License.

using System;
using System.Text;

namespace ImageMagick.Formats.Psd
{
    /// <summary>
    /// The additional info of a <see cref="MagickFormat.Psd"/> image.
    /// </summary>
    public sealed class PsdAdditionalInfo
    {
        private PsdAdditionalInfo(string layerName)
        {
            LayerName = layerName;
        }

        /// <summary>
        /// Gets the name of the layer.
        /// </summary>
        public string LayerName { get; }

        /// <summary>
        /// Creates additional info from a <see cref="MagickFormat.Psd"/> image.
        /// </summary>
        /// <param name="image">The image to create the additonal info from.</param>
        /// <returns>The additional info from a <see cref="MagickFormat.Psd"/> image.</returns>
        public static PsdAdditionalInfo FromImage(IMagickImage image)
        {
            Throw.IfNull(nameof(image), image);

            var profile = image.GetProfile("psd:additional-info");
            if (profile == null)
                return null;

            var bytes = profile.ToByteArray();

            return ParseAdditionalInfo(bytes);
        }

        private static PsdAdditionalInfo ParseAdditionalInfo(byte[] bytes)
        {
            var offset = 0;

            while (offset < bytes.Length - 12)
            {
                offset += 4;
                var key = Encoding.ASCII.GetString(bytes, offset, 4);
                offset += 4;

                int size = bytes[offset++] << 24;
                size |= bytes[offset++] << 16;
                size |= bytes[offset++] << 8;
                size |= bytes[offset++];

                if (offset + size > bytes.Length)
                    break;

                if ("luni".Equals(key, StringComparison.OrdinalIgnoreCase))
                {
                    int count = bytes[offset++] << 24;
                    count |= bytes[offset++] << 16;
                    count |= bytes[offset++] << 8;
                    count |= bytes[offset++];
                    count *= 2;

                    if (count > size - 4)
                        break;

                    SwapBytes(bytes, offset, offset + count);

                    var layerName = Encoding.Unicode.GetString(bytes, offset, count);
                    return new PsdAdditionalInfo(layerName);
                }

                offset += size;
            }

            return null;
        }

        private static void SwapBytes(byte[] bytes, int start, int end)
        {
            for (var i = start + 1; i < end; i += 2)
            {
                var value = bytes[i - 1];
                bytes[i - 1] = bytes[i];
                bytes[i] = value;
            }
        }
    }
}