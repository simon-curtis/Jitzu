#!/usr/bin/env bash
set -euo pipefail

VERSION="${1:?Usage: publish-aur.sh <version> <sha256>}"
HASH="${2:?Usage: publish-aur.sh <version> <sha256>}"

# Setup SSH for AUR
mkdir -p ~/.ssh
echo "$AUR_SSH_PRIVATE_KEY" > ~/.ssh/aur
chmod 600 ~/.ssh/aur
ssh-keyscan -t ed25519 aur.archlinux.org >> ~/.ssh/known_hosts 2>/dev/null
cat > ~/.ssh/config << 'EOF'
Host aur.archlinux.org
  IdentityFile ~/.ssh/aur
  User aur
EOF
chmod 600 ~/.ssh/config

# Clone AUR repo and update
git clone ssh://aur@aur.archlinux.org/jz-bin.git /tmp/aur-repo
cp packaging/arch/PKGBUILD /tmp/aur-repo/PKGBUILD

# Generate .SRCINFO
cat > /tmp/aur-repo/.SRCINFO << SRCINFO
pkgbase = jz-bin
	pkgdesc = The Jitzu programming language interpreter and shell
	pkgver = ${VERSION}
	pkgrel = 1
	url = https://github.com/jitzulang/jitzu
	arch = x86_64
	license = MIT
	depends = glibc
	provides = jz
	conflicts = jz
	source = https://github.com/jitzulang/jitzu/releases/download/v${VERSION}/jitzu-${VERSION}-linux-x64.zip
	sha256sums = ${HASH}

pkgname = jz-bin
SRCINFO

# Commit and push
cd /tmp/aur-repo
git config user.name "Simon Curtis"
git config user.email "simon@jitzu.dev"
git add PKGBUILD .SRCINFO
git diff --cached --quiet && echo "No changes to push" && exit 0
git commit -m "Update to v${VERSION}"
git push
