Remove-Item ..\QuickLook.Plugin.FigmaThumbnailViewer.qlplugin -ErrorAction SilentlyContinue

$files = Get-ChildItem -Path ..\QuickLook.Plugin\QuickLook.Plugin.FigmaThumbnailViewer\bin\Release\ -Exclude *.pdb,*.xml
Compress-Archive $files ..\QuickLook.Plugin.FigmaThumbnailViewer.zip
Move-Item ..\QuickLook.Plugin.FigmaThumbnailViewer.zip ..\QuickLook.Plugin.FigmaThumbnailViewer.qlplugin