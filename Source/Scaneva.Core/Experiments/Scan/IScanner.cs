﻿#region Copyright (C)
// ---------------------------------------------------------------------------------------------------------------
//  <copyright file="Scanner.cs" company="Scaneva">
// 
//  Copyright (C) 2018 Roche Diabetes Care GmbH (Christoph Pieper)
// 
//  This program is free software: you can redistribute it and/or modify
//  it under the terms of the GNU General Public License as published by
//  the Free Software Foundation, either version 3 of the License, or
//  any later version.
//
//  This program is distributed in the hope that it will be useful,
//  but WITHOUT ANY WARRANTY; without even the implied warranty of
//  MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the
//  GNU General Public License for more details.
//
//  You should have received a copy of the GNU General Public License
//  along with this program. If not, see http://www.gnu.org/licenses/.
//  </copyright>
//  <summary>
//  Url: https://github.com/Scaneva
//  </summary>
// --------------------------------------------------------------------------------------------------------------------
#endregion

using System;

namespace Scaneva.Core.Experiments
{
    [Flags]
    public enum enuScannerErrors
    {
        NotInitialized = 0, 
        Initialized = 1,
        Ready = 2,
        Finished = 4,
        Error = 8,
    }

    public interface IScanner
    {

    }
}
