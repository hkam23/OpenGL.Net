﻿
// Copyright (C) 2016 Luca Piccioni
// 
// This library is free software; you can redistribute it and/or
// modify it under the terms of the GNU Lesser General Public
// License as published by the Free Software Foundation; either
// version 2.1 of the License, or (at your option) any later version.
// 
// This library is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the GNU
// Lesser General Public License for more details.
// 
// You should have received a copy of the GNU Lesser General Public
// License along with this library; if not, write to the Free Software
// Foundation, Inc., 51 Franklin Street, Fifth Floor, Boston, MA  02110-1301
// USA

using OpenGL.Objects.State;

namespace OpenGL.Objects.Scene
{
	/// <summary>
	/// Base class for defining scene lighting.
	/// </summary>
	public class SceneObjectLightSpot : SceneObjectLightPoint
	{
		#region Constructors

		/// <summary>
		/// Construct a SceneObjectLightSpot.
		/// </summary>
		public SceneObjectLightSpot()
		{
			
		}

		/// <summary>
		/// Construct a SceneObjectLightSpot.
		/// </summary>
		/// <param name="id">
		/// A <see cref="String"/> that specify the node identifier. It can be null for unnamed objects.
		/// </param>
		public SceneObjectLightSpot(string id) : base(id)
		{
			
		}

		#endregion

		#region Light Model

		/// <summary>
		/// Directoin of the spot.
		/// </summary>
		public Vertex3 Direction;

		/// <summary>
		/// Fall-off angle, in degrees.
		/// </summary>
		public float FalloffAngle;

		/// <summary>
		/// Fall-off exponent.
		/// </summary>
		public float FalloffExponent;

		#endregion

		#region SceneObjectLight Overrides

		/// <summary>
		/// Convert to <see cref="LightsStateBase.Light"/>.
		/// </summary>
		/// <returns>
		/// It returns the <see cref="LightsStateBase.Light"/> corresponding o this Light.
		/// </returns>
		internal override LightsStateBase.Light ToLightState(SceneGraphContext sceneCtx)
		{
			LightsStateBase.Light lightState = base.ToLightState(sceneCtx);

			lightState.FallOff = new Vertex2f(FalloffAngle, FalloffExponent);

			return (lightState);
		}

		#endregion
	}
}
