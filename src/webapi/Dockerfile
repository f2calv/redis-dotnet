# syntax=docker/dockerfile:1
#
# Multi-architecture image, built from a single Dockerfile. Structure follows
# https://github.com/f2calv/multi-arch-container-dotnet
#
# ------------------------------------------------------------------------------
# Stage 1 of 2: build
#
# Pinned to $BUILDPLATFORM and CROSS-COMPILES to $TARGETPLATFORM; emulating the
# target under QEMU instead is typically 10-50x slower.
# ------------------------------------------------------------------------------
FROM --platform=$BUILDPLATFORM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

ARG PROJECT=src/webapi/webapi.csproj
ARG CONFIGURATION=Release

# -- Dependency layer ----------------------------------------------------------
# Copy only what restore reads, so editing a .cs file reuses the cached restore.
# Restore is platform-agnostic, so it precedes TARGETARCH and is shared by every
# architecture.
COPY Directory.Build.props Directory.Packages.props ./
COPY src/SharedLibrary/SharedLibrary.csproj src/SharedLibrary/
COPY src/webapi/webapi.csproj src/webapi/
RUN --mount=type=cache,target=/root/.nuget/packages,sharing=locked \
    dotnet restore "$PROJECT"

# -- Compile layer -------------------------------------------------------------
COPY . .

# buildx injects TARGETARCH/TARGETVARIANT automatically:
#   linux/amd64 -> amd64, linux/arm64 -> arm64, linux/arm/v7 -> arm + v7
# Concatenating the two gives a single flat token to switch on.
ARG TARGETARCH
ARG TARGETVARIANT
RUN --mount=type=cache,target=/root/.nuget/packages,sharing=locked <<EOF
set -eux
# https://learn.microsoft.com/dotnet/core/rid-catalog
case "${TARGETARCH}${TARGETVARIANT}" in
    amd64) RID=linux-x64   ;;
    arm64) RID=linux-arm64 ;;
    armv7) RID=linux-arm   ;;
    *) echo "unsupported platform: linux/${TARGETARCH}/${TARGETVARIANT}" >&2; exit 1 ;;
esac
dotnet publish "$PROJECT" \
    --configuration "$CONFIGURATION" \
    --runtime "$RID" \
    --self-contained false \
    --output /out
EOF

# ------------------------------------------------------------------------------
# Stage 2 of 2: final
#
# No --platform override, so buildx resolves the base image for $TARGETPLATFORM
# and the result is genuinely native to the target.
# ------------------------------------------------------------------------------
FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS final
WORKDIR /app
COPY --from=build /out .

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
LABEL org.opencontainers.image.title="redis-dotnet-webapi" \
    org.opencontainers.image.description="Redis .NET sample Web API" \
    org.opencontainers.image.source="https://github.com/f2calv/redis-dotnet" \
    org.opencontainers.image.licenses="MIT" \
    org.opencontainers.image.version="$GIT_TAG" \
    org.opencontainers.image.revision="$GIT_COMMIT"

# The .NET container images have listened on 8080, not 80, since .NET 8.
EXPOSE 8080

# $APP_UID (1654) is defined by the .NET base images.
USER $APP_UID

ENTRYPOINT ["dotnet", "webapi.dll"]
