#!/bin/bash

# Build standalone executables for Windows, macOS, and Linux
# Covers the all-in-one CLI and each per-database CLI variant

echo "Building standalone Journey CLI executables..."

VERSION="1.1.5"
OUTPUT_DIR="publish"

if [ -d "$OUTPUT_DIR" ]; then
    rm -rf "$OUTPUT_DIR"
fi
mkdir -p "$OUTPUT_DIR"

declare -A VARIANTS=(
    ["journey"]="Journey.Command/Journey.Command.csproj"
    ["journey-postgres"]="Journey.Command.Postgres/Journey.Command.Postgres.csproj"
    ["journey-mysql"]="Journey.Command.MySql/Journey.Command.MySql.csproj"
    ["journey-sqlite"]="Journey.Command.Sqlite/Journey.Command.Sqlite.csproj"
    ["journey-mssql"]="Journey.Command.Mssql/Journey.Command.Mssql.csproj"
    ["journey-cassandra"]="Journey.Command.Cassandra/Journey.Command.Cassandra.csproj"
)

PLATFORMS=("win-x64" "win-arm64" "osx-x64" "osx-arm64" "linux-x64" "linux-arm64")

for NAME in "${!VARIANTS[@]}"; do
    PROJECT="${VARIANTS[$NAME]}"
    echo ""
    echo "Building $NAME..."
    for PLATFORM in "${PLATFORMS[@]}"; do
        DIR="$OUTPUT_DIR/$NAME-$PLATFORM"
        echo "  [$PLATFORM]"
        dotnet publish "$PROJECT" -c Release -r "$PLATFORM" -f net9.0 -o "$DIR" \
            --self-contained true -p:PublishSingleFile=true -p:Version="$VERSION" --nologo -v q

        case "$PLATFORM" in
            win-*)
                zip -qr "$OUTPUT_DIR/${NAME}_${VERSION}_${PLATFORM}.zip" "$DIR"
                ;;
            *)
                tar -czf "$OUTPUT_DIR/${NAME}_${VERSION}_${PLATFORM}.tgz" -C "$OUTPUT_DIR" "$NAME-$PLATFORM"
                ;;
        esac
        echo "    -> $OUTPUT_DIR/${NAME}_${VERSION}_${PLATFORM}"
    done
done

echo ""
echo "Done. Archives in '$OUTPUT_DIR/':"
ls -lh "$OUTPUT_DIR"/*.zip "$OUTPUT_DIR"/*.tgz 2>/dev/null
