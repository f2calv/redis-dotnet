# syntax=docker/dockerfile:1
#
# Multi-architecture image for the webapi sample, built from a single Dockerfile.
# Structure follows https://github.com/f2calv/multi-arch-container-dotnet
#
# Build from the repository root, which is the build context:
#
#   docker buildx build -f src/webapi/Dockerfile .
#
# ------------------------------------------------------------------------------
# Stage 1 of 2: build
#
# Pinned to $BUILDPLATFORM and CROSS-COMPILES to $TARGETPLATFORM; emulating the
# target under QEMU instead is often an order of magnitude slower.
# ------------------------------------------------------------------------------
FROM --platform=$BUILDPLATFORM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

ARG APP_NAME=webapi
ARG CONFIGURATION=Release

# -- Dependency layer ----------------------------------------------------------
# Copy ONLY the files that influence `dotnet restore` (--parents preserves the
# directory structure) so that editing a .cs file reuses the cached restore.
# Restore is platform-agnostic, so it runs before TARGETARCH and is shared by
# every architecture; restoring all three runtime identifiers lets each
# platform's publish run offline with --no-restore.
COPY --parents Directory.Build.props Directory.Packages.props global.json src/**/*.csproj ./
RUN --mount=type=cache,target=/root/.nuget/packages,sharing=locked \
    dotnet restore "src/$APP_NAME/$APP_NAME.csproj" \
    "-p:RuntimeIdentifiers=\"linux-x64;linux-arm64;linux-arm\""

# -- Compile layer -------------------------------------------------------------
# Test projects are excluded so a test edit never invalidates the publish layer.
COPY --exclude=src/*.Tests . .

# buildx injects TARGETARCH/TARGETVARIANT automatically:
#   linux/amd64  -> TARGETARCH=amd64  TARGETVARIANT=
#   linux/arm64  -> TARGETARCH=arm64  TARGETVARIANT=
#   linux/arm/v7 -> TARGETARCH=arm    TARGETVARIANT=v7
# The publish only reads packages the restore already wrote, so the platform
# legs share the cache and need no network.
ARG TARGETARCH
ARG TARGETVARIANT
RUN --network=none --mount=type=cache,target=/root/.nuget/packages,sharing=shared <<EOF
set -eux
# https://learn.microsoft.com/dotnet/core/rid-catalog
case "${TARGETARCH}${TARGETVARIANT}" in
    amd64) RID=linux-x64   ;;
    arm64) RID=linux-arm64 ;;
    armv7) RID=linux-arm   ;;
    *) echo "unsupported platform: linux/${TARGETARCH}/${TARGETVARIANT}" >&2; exit 1 ;;
esac
dotnet publish "src/$APP_NAME/$APP_NAME.csproj" \
    --configuration "$CONFIGURATION" \
    --runtime "$RID" \
    --self-contained false \
    --no-restore \
    --output /out
EOF

# ------------------------------------------------------------------------------
# Stage 2 of 2: final
#
# No --platform override here, so buildx resolves the base image for
# $TARGETPLATFORM and the resulting image is genuinely native to the target.
#
# Alternatives, smallest to largest: aspnet:10.0-noble-chiseled (used here: no shell,
# no package manager, non-root), aspnet:10.0-alpine (musl, no tzdata by default) and
# aspnet:10.0 (full Ubuntu). Nothing here needs a shell or an apt package.
# ------------------------------------------------------------------------------
FROM mcr.microsoft.com/dotnet/aspnet:10.0-noble-chiseled AS final
WORKDIR /app
COPY --link --from=build /out .

EXPOSE 8080

# -- Provenance ----------------------------------------------------------------
ARG GIT_REPOSITORY=n/a
ENV GIT_REPOSITORY=$GIT_REPOSITORY
ARG GIT_BRANCH=n/a
ENV GIT_BRANCH=$GIT_BRANCH
ARG GIT_COMMIT=n/a
ENV GIT_COMMIT=$GIT_COMMIT
ARG GIT_TAG=n/a
ENV GIT_TAG=$GIT_TAG

ARG GITHUB_WORKFLOW=n/a
ENV GITHUB_WORKFLOW=$GITHUB_WORKFLOW
ARG GITHUB_RUN_ID=0
ENV GITHUB_RUN_ID=$GITHUB_RUN_ID
ARG GITHUB_RUN_NUMBER=0
ENV GITHUB_RUN_NUMBER=$GITHUB_RUN_NUMBER

# https://github.com/opencontainers/image-spec/blob/main/annotations.md
LABEL org.opencontainers.image.title="webapi" \
    org.opencontainers.image.description="Redis with .NET: web API sample" \
    org.opencontainers.image.source="https://github.com/f2calv/redis-dotnet" \
    org.opencontainers.image.licenses="Unlicense" \
    org.opencontainers.image.version="$GIT_TAG" \
    org.opencontainers.image.revision="$GIT_COMMIT"

# $APP_UID is defined by the .NET base images; chiselled images already run as it.
USER $APP_UID

ENTRYPOINT ["dotnet", "webapi.dll"]
