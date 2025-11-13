#!/bin/bash

# WinTail - macOS Build Script
# This script builds WinTail for macOS (both Intel and Apple Silicon)

set -e  # Exit on error

echo "?? WinTail macOS Build Script"
echo "================================"
echo ""

# Detect architecture
ARCH=$(uname -m)
if [ "$ARCH" = "arm64" ]; then
    RID="osx-arm64"
    ARCH_NAME="Apple Silicon (M1/M2/M3/M4)"
else
    RID="osx-x64"
    ARCH_NAME="Intel"
fi

echo "?? Detected Architecture: $ARCH_NAME"
echo "?? Build Target: $RID"
echo "?? Framework: .NET 10"
echo ""

# Check if .NET 10 is installed
if ! dotnet --list-sdks | grep -q "10\."; then
    echo "? Error: .NET 10 SDK is not installed"
    echo "Please install .NET 10 SDK from: https://dotnet.microsoft.com/download/dotnet/10.0"
    exit 1
fi

# Clean previous builds
echo "?? Cleaning previous builds..."
dotnet clean --configuration Release > /dev/null

# Restore dependencies
echo "?? Restoring dependencies..."
dotnet restore

# Build
echo "?? Building for Release..."
dotnet build --configuration Release --no-restore

# Publish
echo "?? Publishing self-contained app..."
dotnet publish -c Release -r $RID \
    --self-contained true \
    -p:PublishSingleFile=true \
    -p:PublishTrimmed=true \
    -p:EnableCompressionInSingleFile=true \
    -p:DebugType=None \
    -p:DebugSymbols=false

PUBLISH_DIR="bin/Release/net10.0/$RID/publish"

echo ""
echo "? Build complete!"
echo "?? Output directory: $PUBLISH_DIR"
echo ""

# Create .app bundle
echo "?? Creating macOS .app bundle..."

APP_NAME="WinTail.app"
APP_DIR="$PUBLISH_DIR/$APP_NAME"

# Remove old bundle if exists
rm -rf "$APP_DIR"

# Create bundle structure
mkdir -p "$APP_DIR/Contents/MacOS"
mkdir -p "$APP_DIR/Contents/Resources"

# Copy executable and all files
cp "$PUBLISH_DIR/WinTail.Avalonia" "$APP_DIR/Contents/MacOS/"
find "$PUBLISH_DIR" -maxdepth 1 -type f ! -name "WinTail.Avalonia" -exec cp {} "$APP_DIR/Contents/MacOS/" \;

# Create Info.plist
cat > "$APP_DIR/Contents/Info.plist" << 'PLIST'
<?xml version="1.0" encoding="UTF-8"?>
<!DOCTYPE plist PUBLIC "-//Apple//DTD PLIST 1.0//EN" "http://www.apple.com/DTDs/PropertyList-1.0.dtd">
<plist version="1.0">
<dict>
    <key>CFBundleExecutable</key>
    <string>WinTail.Avalonia</string>
    <key>CFBundleIdentifier</key>
    <string>com.wintail.avalonia</string>
    <key>CFBundleName</key>
    <string>WinTail</string>
    <key>CFBundleDisplayName</key>
    <string>WinTail</string>
    <key>CFBundleVersion</key>
    <string>1.0.0</string>
    <key>CFBundleShortVersionString</key>
    <string>1.0.0</string>
    <key>CFBundlePackageType</key>
    <string>APPL</string>
    <key>LSMinimumSystemVersion</key>
    <string>10.15</string>
    <key>NSHighResolutionCapable</key>
    <true/>
    <key>NSPrincipalClass</key>
    <string>NSApplication</string>
    <key>LSApplicationCategoryType</key>
    <string>public.app-category.utilities</string>
    <key>CFBundleDocumentTypes</key>
    <array>
        <dict>
            <key>CFBundleTypeName</key>
            <string>Log File</string>
            <key>CFBundleTypeRole</key>
            <string>Viewer</string>
            <key>CFBundleTypeExtensions</key>
            <array>
                <string>log</string>
                <string>txt</string>
                <string>vlog</string>
            </array>
            <key>LSHandlerRank</key>
            <string>Alternate</string>
        </dict>
    </array>
</dict>
</plist>
PLIST

# Make executable
chmod +x "$APP_DIR/Contents/MacOS/WinTail.Avalonia"

# Remove quarantine attribute
xattr -cr "$APP_DIR" 2>/dev/null || true

echo "? App bundle created: $APP_DIR"
echo ""

# Display size
SIZE=$(du -sh "$APP_DIR" | cut -f1)
echo "?? App bundle size: $SIZE"
echo ""

# Instructions
echo "?? Build Complete!"
echo ""
echo "To run the app:"
echo "  open $APP_DIR"
echo ""
echo "Or from command line:"
echo "  ./$APP_DIR/Contents/MacOS/WinTail.Avalonia"
echo ""
echo "To install to Applications:"
echo "  cp -R $APP_DIR /Applications/"
echo ""
echo "To open a log file:"
echo "  open -a $APP_DIR /path/to/file.log"
echo ""
echo "?? Note: If macOS blocks the app, go to System Settings > Privacy & Security"
echo "   and click 'Open Anyway' to allow WinTail to run."
echo ""
