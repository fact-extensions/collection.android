#!/bin/bash

. ../ext/useful-scripts/setenv
. ../ext/myget/setenv_nuget.sh

export VERSION_PREFIX=1.0.0
export VERSION_SUFFIX=alpha011
export VERSION=$VERSION_PREFIX-$VERSION_SUFFIX
