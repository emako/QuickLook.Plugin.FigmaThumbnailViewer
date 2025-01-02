// Copyright © 2024 ema
//
// This file is part of QuickLook program.
//
// This program is free software: you can redistribute it and/or modify
// it under the terms of the GNU General Public License as published by
// the Free Software Foundation, either version 3 of the License, or
// (at your option) any later version.
//
// This program is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
// GNU General Public License for more details.
//
// You should have received a copy of the GNU General Public License
// along with this program.  If not, see <http://www.gnu.org/licenses/>.

using PureSharpCompress.Archives.Zip;
using PureSharpCompress.Common;
using PureSharpCompress.Readers;
using QuickLook.Common.Plugin;
using System;
using System.IO;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media.Imaging;
using System.Windows.Resources;

namespace QuickLook.Plugin.FigmaThumbnailViewer;

public class Plugin : IViewer
{
    private ImagePanel? _ip;

    public int Priority => 0;

    public void Init()
    {
    }

    public bool CanHandle(string path)
    {
        return Path.GetExtension(path.ToLower()) == ".fig";
    }

    public void Prepare(string path, ContextObject context)
    {
        try
        {
            using Stream imageData = ViewImage(path);
            BitmapImage bitmap = imageData.ReadAsBitmapImage();
            context.PreferredSize = new Size { Width = bitmap.PixelWidth * 1.4d, Height = bitmap.PixelHeight * 1.8d };
        }
        catch (Exception e)
        {
            _ = e;
            context.PreferredSize = new Size { Width = 100, Height = 100 };
        }
    }

    public void View(string path, ContextObject context)
    {
        _ip = new ImagePanel
        {
            ContextObject = context,
        };

        _ = Task.Run(() =>
        {
            using Stream imageData = ViewImage(path);
            BitmapImage bitmap = imageData.ReadAsBitmapImage();

            if (_ip is null) return;

            _ip.Dispatcher.Invoke(() =>
            {
                _ip.Source = bitmap;
                _ip.DoZoomToFit();
            });
            context.IsBusy = false;
            context.Title = $"{bitmap.PixelWidth}x{bitmap.PixelHeight}: {Path.GetFileName(path)}";
        });

        context.ViewerContent = _ip;
        context.Title = $"{Path.GetFileName(path)}";
    }

    public void Cleanup()
    {
        GC.SuppressFinalize(this);

        _ip = null;
    }

    public static Stream ViewImage(string path)
    {
        try
        {
            using ZipArchive? archive = ZipArchive.Open(path, new());
            using IReader reader = archive.ExtractAllEntries();

            while (reader.MoveToNextEntry())
            {
                if (reader.Entry.Key!.Equals("thumbnail.png", StringComparison.OrdinalIgnoreCase))
                {
                    MemoryStream ms = new();
                    using EntryStream stream = reader.OpenEntryStream();
                    stream.CopyTo(ms);
                    return ms;
                }
            }
        }
        catch
        {
            ///
        }

        StreamResourceInfo info = Application.GetResourceStream(new Uri("pack://application:,,,/QuickLook.Plugin.FigmaThumbnailViewer;component/Resources/broken.png"));
        return info?.Stream!;
    }
}

file static class Extension
{
    public static BitmapImage ReadAsBitmapImage(this Stream imageData)
    {
        imageData.Seek(0U, SeekOrigin.Begin);

        BitmapImage bitmap = new();
        bitmap.BeginInit();
        bitmap.CacheOption = BitmapCacheOption.OnLoad;
        bitmap.StreamSource = imageData;
        bitmap.EndInit();
        bitmap.Freeze();
        return bitmap;
    }
}
