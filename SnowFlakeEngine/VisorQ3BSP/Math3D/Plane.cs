#region GPL License

/*
Copyright (c) 2010 Miguel Angel Guirado López

This file is part of Math3D.

    Math3D is free software: you can redistribute it and/or modify
    it under the terms of the GNU General Public License as published by
    the Free Software Foundation, either version 3 of the License, or
    (at your option) any later version.

    Math3D is distributed in the hope that it will be useful,
    but WITHOUT ANY WARRANTY; without even the implied warranty of
    MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
    GNU General Public License for more details.

    You should have received a copy of the GNU General Public License
    along with Math3D.  If not, see <http://www.gnu.org/licenses/>.
*/

#endregion

#region LGPL License

/*
Axiom Graphics Engine Library
Copyright (C) 2003-2006 Axiom Project Team

The overall design, and a majority of the core engine and rendering code 
contained within this library is a derivative of the open source Object Oriented 
Graphics Engine OGRE, which can be found at http://ogre.sourceforge.net.  
Many thanks to the OGRE team for maintaining such a high quality project.

The math library included in this project, in addition to being a derivative of
the works of Ogre, also include derivative work of the free portion of the 
Wild Magic mathematics source code that is distributed with the excellent
book Game Engine Design.
http://www.wild-magic.com/

This library is free software; you can redistribute it and/or
modify it under the terms of the GNU Lesser General Public
License as published by the Free Software Foundation; either
version 2.1 of the License, or (at your option) any later version.

This library is distributed in the hope that it will be useful,
but WITHOUT ANY WARRANTY; without even the implied warranty of
MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU
Lesser General Public License for more details.

You should have received a copy of the GNU Lesser General Public
License along with this library; if not, write to the Free Software
Foundation, Inc., 59 Temple Place, Suite 330, Boston, MA 02111-1307 USA
*/

#endregion

#region SVN Version Information

// <file>
//     <license see="http://axiomengine.sf.net/wiki/index.php/license.txt"/>
//     <id value="$Id: Plane.cs 1004 2007-01-02 18:57:49Z borrillis $"/>
// </file>

#endregion SVN Version Information

#region Namespace Declarations

using System;

#endregion Namespace Declarations

namespace Math3D
{
    /// <summary>
    ///     Defines a plane in 3D space.
    /// </summary>
    /// <remarks>
    ///     A plane is defined in 3D space by the equation
    ///     Ax + By + Cz + D = 0
    ///     This equates to a vector (the normal of the plane, whose x, y
    ///     and z components equate to the coefficients A, B and C
    ///     respectively), and a constant (D) which is the distance along
    ///     the normal you have to go to move the plane back to the origin.
    /// </remarks>
    [Serializable]
    public class Plane
    {
        #region Fields

        /// <summary>
        ///     Direction the plane is facing.
        /// </summary>
        public Vector3f Normal;

        /// <summary>
        ///     Distance from the origin.
        /// </summary>
        public float D;

        #endregion Fields

        #region Constructors

        public Plane(Plane plane)
        {
            Normal = new Vector3f();
            Normal.CopyFrom(plane.Normal);
            D = plane.D;
        }

        /// <summary>
        ///     Construct a plane through a normal, and a distance to move the plane along the normal.
        /// </summary>
        /// <param name="normal"></param>
        /// <param name="constant"></param>
        public Plane(Vector3f normal, float constant)
        {
            Normal = normal;
            D = -constant;
        }

        public Plane(Vector3f normal, Vector3f point)
        {
            Normal = normal;
            D = -normal.Dot(point);
        }

        /// <summary>
        ///     Construct a plane from 3 coplanar points.
        /// </summary>
        /// <param name="point0">First point.</param>
        /// <param name="point1">Second point.</param>
        /// <param name="point2">Third point.</param>
        public Plane(Vector3f point0, Vector3f point1, Vector3f point2)
        {
            Normal = new Vector3f();
            var edge1 = point1 - point0;
            var edge2 = point2 - point0;
            Vector3f.Cross(edge1, edge2, ref Normal);
            Normal.Normalize();
            D = -Normal.Dot(point0);
        }

        #endregion

        #region Propiedades

        public float A
        {
            get { return Normal.X; }
        }

        public float B
        {
            get { return Normal.Y; }
        }

        public float C
        {
            get { return Normal.Z; }
        }

        #endregion Propiedades

        #region Methods

        /// <summary>
        /// </summary>
        /// <param name="point"></param>
        /// <returns></returns>
        public PlaneSide GetSide(Vector3f point)
        {
            var distance = GetDistance(point);

            if (distance < 0.0f)
                return PlaneSide.Negative;

            if (distance > 0.0f)
                return PlaneSide.Positive;

            return PlaneSide.None;
        }

        /// <summary>
        ///     This is a pseudodistance. The sign of the return value is
        ///     positive if the point is on the positive side of the plane,
        ///     negative if the point is on the negative side, and zero if the
        ///     point is on the plane.
        ///     The absolute value of the return value is the true distance only
        ///     when the plane normal is a unit length vector.
        /// </summary>
        /// <param name="point"></param>
        /// <returns></returns>
        public float GetDistance(Vector3f point)
        {
            return Normal.Dot(point) + D;
        }

        /// <summary>
        ///     Construct a plane from 3 coplanar points.
        /// </summary>
        /// <param name="point0">First point.</param>
        /// <param name="point1">Second point.</param>
        /// <param name="point2">Third point.</param>
        public void Redefine(Vector3f point0, Vector3f point1, Vector3f point2)
        {
            var edge1 = point1 - point0;
            var edge2 = point2 - point0;
            Vector3f.Cross(edge1, edge2, ref Normal);
            Normal.Normalize();
            D = -Normal.Dot(point0);
        }

        #endregion Methods

        #region Object overrides

        /// <summary>
        ///     Object method for testing equality.
        /// </summary>
        /// <param name="obj">Object to test.</param>
        /// <returns>True if the 2 planes are logically equal, false otherwise.</returns>
        public override bool Equals(object obj)
        {
            var plane = (Plane) obj;

            return plane == this;
        }

        /// <summary>
        ///     Gets the hashcode for this Plane.
        /// </summary>
        /// <returns></returns>
        public override int GetHashCode()
        {
            return D.GetHashCode() ^ Normal.GetHashCode();
        }

        /// <summary>
        ///     Returns a string representation of this Plane.
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            return string.Format("Distance: {0} Normal: {1}", D, Normal);
        }

        #endregion

        #region Operator Overloads

        /// <summary>
        ///     Compares 2 Planes for equality.
        /// </summary>
        /// <param name="left">First plane.</param>
        /// <param name="right">Second plane.</param>
        /// <returns>true if equal, false if not equal.</returns>
        public static bool operator ==(Plane left, Plane right)
        {
            return (left.D == right.D) && (left.Normal == right.Normal);
        }

        /// <summary>
        ///     Compares 2 Planes for inequality.
        /// </summary>
        /// <param name="left">First plane.</param>
        /// <param name="right">Second plane.</param>
        /// <returns>true if not equal, false if equal.</returns>
        public static bool operator !=(Plane left, Plane right)
        {
            return (left.D != right.D) || (left.Normal != right.Normal);
        }

        #endregion
    }

    /// <summary>
    ///     The "positive side" of the plane is the half space to which the
    ///     plane normal points. The "negative side" is the other half
    ///     space. The flag "no side" indicates the plane itself.
    /// </summary>
    public enum PlaneSide
    {
        None,
        Positive,
        Negative
    }
}