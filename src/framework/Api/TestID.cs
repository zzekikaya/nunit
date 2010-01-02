// ***********************************************************************
// Copyright (c) 2007 Charlie Poole
//
// Permission is hereby granted, free of charge, to any person obtaining
// a copy of this software and associated documentation files (the
// "Software"), to deal in the Software without restriction, including
// without limitation the rights to use, copy, modify, merge, publish,
// distribute, sublicense, and/or sell copies of the Software, and to
// permit persons to whom the Software is furnished to do so, subject to
// the following conditions:
// 
// The above copyright notice and this permission notice shall be
// included in all copies or substantial portions of the Software.
// 
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND,
// EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF
// MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND
// NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE
// LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION
// OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION
// WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
// ***********************************************************************

using System;

namespace NUnit.Framework.Api
{
	/// <summary>
	/// TestID encapsulates a unique identifier for tests. As
	/// currently implemented, this is an integer and is unique
	/// within the AppDomain. TestID is one component of a 
	/// TestName. We use this object, rather than a raw int,
	/// for two reasons: (1) to hide the implementation so
	/// it may be changed later if necessary and (2) so that the
	/// id may be null in a "weak" TestName.
	/// </summary>
	[Serializable]
	public class TestID : ICloneable
	{
		#region Fields
			/// <summary>
			/// The int key that distinguishes this test from all others created
			/// by the same runner.
			/// </summary>
			private int id;
		
		/// <summary>
		/// Static value to seed ids. It's started at 1000 so any
		/// uninitialized ids will stand out.
		/// </summary>
		private static int nextID = 1000;

		#endregion

		#region Construction
		/// <summary>
		/// Construct a new TestID
		/// </summary>
		public TestID()
		{
			this.id = unchecked( nextID++ );
		}

		/// <summary>
		/// Construct a TestID with a given value.
		/// Used in parsing test names and in order
		/// to construct an artificial test node for
		/// aggregating multiple test runners.
		/// </summary>
		/// <param name="id"></param>
		public TestID( int id )
		{
			this.id = id;
		}
		#endregion

		#region Static Methods
		/// <summary>
		/// Parse a TestID from it's string representation
		/// </summary>
		/// <param name="s"></param>
		/// <returns></returns>
		public static TestID Parse( string s )
		{
			int id = Int32.Parse( s );
			return new TestID( id );
		}
		#endregion

		#region Object Overrides
		/// <summary>
		/// Override of Equals method to allow comparison of TestIDs
		/// </summary>
		/// <param name="obj"></param>
		/// <returns></returns>
		public override bool Equals(object obj)
		{
			TestID other = obj as TestID;
			if ( other != null )
				return this.id == other.id;

			return base.Equals (obj);
		}

		/// <summary>
		/// Override of GetHashCode for TestIDs
		/// </summary>
		/// <returns></returns>
		public override int GetHashCode()
		{
			return id.GetHashCode();
		}

		/// <summary>
		/// Override ToString() to display the int id
		/// </summary>
		/// <returns></returns>
		public override string ToString()
		{
			return id.ToString();
		}
		#endregion

		#region Operator Overrides
        /// <summary>
        /// Operator == override
        /// </summary>
        /// <param name="id1"></param>
        /// <param name="id2"></param>
        /// <returns></returns>
		public static bool operator ==( TestID id1, TestID id2 )
		{
			if ( Object.Equals( id1, null ) )
				return Object.Equals( id2, null );

			return id1.Equals( id2 );
		}

        /// <summary>
        /// Operator != override
        /// </summary>
        /// <param name="id1"></param>
        /// <param name="id2"></param>
        /// <returns></returns>
		public static bool operator !=( TestID id1, TestID id2 )
		{
			return id1 == id2 ? false : true;
		}
		#endregion

		#region ICloneable Implementation
        /// <summary>
        /// Clone this TestID
        /// </summary>
        /// <returns>An identical TestID</returns>
		public object Clone()
		{
			return this.MemberwiseClone();
		}
		#endregion
	}
}
