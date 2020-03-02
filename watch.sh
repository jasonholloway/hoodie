#!/bin/bash
unbuffer dotnet watch run | grep -v warning
