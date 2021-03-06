﻿/* This Source Code Form is subject to the terms of the Mozilla Public
* License, v. 2.0. If a copy of the MPL was not distributed with this
* file, You can obtain one at http://mozilla.org/MPL/2.0/. */

// Copyright (C) 2016 Cameron Angus. All Rights Reserved.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Diagnostics;

namespace KantanDocGen
{
	public abstract class DocXform
	{
		public abstract bool Initialize(string XsltPath, DataReceivedEventHandler OutputHandler);
		public abstract bool TransformXml(string SourceXmlPath, string OutputPath);
	}
}
