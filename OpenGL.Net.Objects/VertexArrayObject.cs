
// Copyright (C) 2011-2016 Luca Piccioni
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

using System;
using System.Diagnostics;

namespace OpenGL.Objects
{
	/// <summary>
	/// A collection of vertex arrays and vertex indices.
	/// </summary>
	/// <remarks>
	/// <para>
	/// Vertex arrays are collected by semantic or name. The array buffers corresponding to vertex attributes
	/// can have any memory layout (offset and interleaving).
	/// </para>
	/// <para>
	/// Vertex arrays are rendered using a collection of vertex elements. Each vertex element issue a rendering
	/// command, referencing the vertex array data directly or using a deferencing index.
	/// </para>
	/// <para>
	/// A vertex array can have associated a shader program, and an render state set. Those two objects defines
	/// the rendering result of the collected arrays.
	/// </para>
	/// </remarks>
	public partial class VertexArrayObject : GraphicsResource, IBindingResource
	{
		#region Draw

		/// <summary>
		/// Draw the attributes of this vertex array.
		/// </summary>
		/// <param name="ctx">
		/// The <see cref="GraphicsContext"/> used for rendering.
		/// </param>
		public void Draw(GraphicsContext ctx)
		{
			Draw(ctx, null);
		}

		/// <summary>
		/// Draw the attributes of this vertex array.
		/// </summary>
		/// <param name="ctx">
		/// The <see cref="GraphicsContext"/> used for rendering.
		/// </param>
		/// <param name="shader">
		/// The <see cref="ShaderProgram"/> used for drawing the vertex arrays.
		/// </param>
		public virtual void Draw(GraphicsContext ctx, ShaderProgram shader)
		{
			CheckThisExistence(ctx);

			if (shader != null && shader.Exists(ctx) == false)
				throw new ArgumentException("not existing", "shader");

			// If vertex was modified after creation, don't miss to create array buffers
			if (_VertexArrayDirty) CreateObject(ctx);

			// Set vertex arrays
			SetVertexArrayState(ctx, shader);
			
			// Fixed or programmable pipeline?
			if (shader != null)
				ctx.Bind(shader);
			else
				ctx.ResetProgram();

			// Issue rendering using shader
			foreach (Element attributeElement in DrawElements) {
				if (_FeedbackBuffer != null)
					_FeedbackBuffer.Begin(ctx, attributeElement.ElementsMode);

				attributeElement.Draw(ctx);
				
				if (_FeedbackBuffer != null)
					_FeedbackBuffer.End(ctx);
			}
		}

		/// <summary>
		/// Set the vertex arrays state for the shader program.
		/// </summary>
		/// <param name="ctx">
		/// A <see cref="GraphicsContext"/> on which the shader program is bound.
		/// </param>
		/// <param name="shaderProgram">
		/// A <see cref="ShaderProgram"/> on which the vertex arrays shall be bound.
		/// </param>
		protected void SetVertexArrayState(GraphicsContext ctx, ShaderProgram shaderProgram)
		{
			CheckThisExistence(ctx);

			ctx.Bind(this);

			// Note: when the GL_ARB_vertex_array_object is supported, there's no need
			// to define vertex attribute each time
			if (Gl.CurrentExtensions.VertexArrayObject_ARB && !_VertexArrayDirty) {
				// CheckVertexAttributes(ctx, shaderProgram);
				return;
			}

			if (shaderProgram != null) {
				uint attributesSet = 0;

				// Set vertex array state
				foreach (string attributeName in shaderProgram.ActiveAttributes) {
					IVertexArray shaderVertexArray = GetVertexArray(attributeName, shaderProgram);
					if (shaderVertexArray == null)
						continue;

					// Set array attribute
					shaderVertexArray.SetVertexAttribute(ctx, shaderProgram, attributeName); attributesSet++;
				}

				if (attributesSet == 0)
					throw new InvalidOperationException("no attribute is set");
			} else {
				IVertexArray attributeArray;

				// No shader program: using fixed pipeline program. ATM enable all attributes having a semantic
				if ((attributeArray = GetVertexArray(VertexArraySemantic.Position)) == null)
					throw new InvalidOperationException("no position semantic array defined");
				attributeArray.SetVertexAttribute(ctx, null, VertexArraySemantic.Position);

				// Optional attributes
				if ((attributeArray = GetVertexArray(VertexArraySemantic.Color)) != null)
					attributeArray.SetVertexAttribute(ctx, null, VertexArraySemantic.Color);
				if ((attributeArray = GetVertexArray(VertexArraySemantic.Normal)) != null)
					attributeArray.SetVertexAttribute(ctx, null, VertexArraySemantic.Normal);
				if ((attributeArray = GetVertexArray(VertexArraySemantic.TexCoord)) != null)
					attributeArray.SetVertexAttribute(ctx, null, VertexArraySemantic.TexCoord);
			}

			// Next time do not set inputs if GL_ARB_vertex_array_object is supported
			_VertexArrayDirty = false;
		}

		/// <summary>
		/// Utility routine for checking the vertex array state.
		/// </summary>
		/// <param name="ctx">
		/// A <see cref="GraphicsContext"/> on which the shader program is bound.
		/// </param>
		/// <param name="shaderProgram">
		/// A <see cref="ShaderProgram"/> on which the vertex arrays shall be bound.
		/// </param>
		private void CheckVertexAttributes(GraphicsContext ctx, ShaderProgram shaderProgram)
		{
			// Vertex array state overall check
			foreach (string attributeName in shaderProgram.ActiveAttributes) {
				ShaderProgram.AttributeBinding attributeBinding = shaderProgram.GetActiveAttribute(attributeName);
				VertexArray shaderVertexArray = GetVertexArray(attributeName, shaderProgram) as VertexArray;
				if (shaderVertexArray == null)
					continue;

				shaderVertexArray.CheckVertexAttribute(ctx, attributeBinding);
			}
		}
	
		/// <summary>
		/// Flag indicating whether the vertex array is dirty due buffer object changes.
		/// </summary>
		private bool _VertexArrayDirty = true;

		#endregion

		#region Draw Instanced

		/// <summary>
		/// Draw a number of instances of the attributes of this vertex array.
		/// </summary>
		/// <param name="ctx">
		/// The <see cref="GraphicsContext"/> used for rendering.
		/// </param>
		/// <param name="instances">
		/// A <see cref="UInt32"/> that specify the number of instances to draw.
		/// </param>
		public virtual void DrawInstanced(GraphicsContext ctx, uint instances)
		{
			DrawInstanced(ctx, null, instances);
		}

		/// <summary>
		/// Draw a number of instances of the attributes of this vertex array.
		/// </summary>
		/// <param name="ctx">
		/// The <see cref="GraphicsContext"/> used for rendering.
		/// </param>
		/// <param name="shader">
		/// The <see cref="ShaderProgram"/> used for drawing the vertex arrays.
		/// </param>
		/// <param name="instances">
		/// A <see cref="UInt32"/> that specify the number of instances to draw.
		/// </param>
		public virtual void DrawInstanced(GraphicsContext ctx, ShaderProgram shader, uint instances)
		{
			CheckThisExistence(ctx);

			if (shader != null && shader.Exists(ctx) == false)
				throw new ArgumentException("not existing", "shader");

			// If vertex was modified after creation, don't miss to create array buffers
			if (_VertexArrayDirty) CreateObject(ctx);

			// Set vertex arrays
			SetVertexArrayState(ctx, shader);

			// Fixed or programmable pipeline?
			if (shader != null)
				ctx.Bind(shader);
			else
				ctx.ResetProgram();

			// Issue rendering using shader
			foreach (Element attributeElement in DrawElements) {
				if (_FeedbackBuffer != null)
					_FeedbackBuffer.Begin(ctx, attributeElement.ElementsMode);

				attributeElement.DrawInstanced(ctx, instances);

				if (_FeedbackBuffer != null)
					_FeedbackBuffer.End(ctx);
			}
		}

		#endregion

		#region Transform Feedback

		public void SetTransformFeedback(FeedbackBufferObject feedbackObject)
		{
			if (_FeedbackBuffer != null)
				_FeedbackBuffer.DecRef();
		
			_FeedbackBuffer = feedbackObject;
		
			if (_FeedbackBuffer != null)
				_FeedbackBuffer.IncRef();
		}
	
		/// <summary>
		/// The feedback buffer object that specify the feedback array buffers.
		/// </summary>
		private FeedbackBufferObject _FeedbackBuffer;
		
		#endregion

		#region GraphicsResource Overrides

		/// <summary>
		/// Vertex array object class.
		/// </summary>
		internal static readonly Guid ThisObjectClass = new Guid("821E9AC8-6118-4543-B561-7C85BB998287");

		/// <summary>
		/// Vertex array object class.
		/// </summary>
		public override Guid ObjectClass { get { return (ThisObjectClass); } }

		/// <summary>
		/// Determine whether this BufferObject really exists for a specific context.
		/// </summary>
		/// <param name="ctx">
		/// A <see cref="GraphicsContext"/> that would have created (or a sharing one) the object. This context shall be current to
		/// the calling thread.
		/// </param>
		/// <returns>
		/// It returns a boolean value indicating whether this BufferObject exists in the object space of <paramref name="ctx"/>.
		/// </returns>
		/// <remarks>
		/// <para>
		/// The object existence is done by checking a valid object by its name <see cref="IGraphicsResource.ObjectName"/>. This routine will test whether
		/// <paramref name="ctx"/> has created this BufferObject (or is sharing with the creator).
		/// </para>
		/// </remarks>
		/// <exception cref="ArgumentNullException">
		/// Exception thrown if <paramref name="ctx"/> is null.
		/// </exception>
		/// <exception cref="ArgumentException">
		/// Exception thrown if <paramref name="ctx"/> is not current to the calling thread.
		/// </exception>
		public override bool Exists(GraphicsContext ctx)
		{
			if (ctx == null)
				throw new ArgumentNullException("ctx");
			if (ctx.IsCurrent == false)
				throw new ArgumentException("not current", "ctx");

			// Object name space test (and 'ctx' sanity checks)
			if (base.Exists(ctx) == false)
				return (false);

			return (!RequiresName(ctx) || Gl.IsVertexArray(ObjectName));
		}

		/// <summary>
		/// Determine whether this object requires a name bound to a context or not.
		/// </summary>
		/// <param name="ctx">
		/// A <see cref="GraphicsContext"/> used for creating this object name.
		/// </param>
		/// <returns>
		/// It returns always false. Names are managed manually
		/// </returns>
		protected override bool RequiresName(GraphicsContext ctx)
		{
			if (ctx == null)
				throw new ArgumentNullException("ctx");

			return (Gl.CurrentExtensions.VertexArrayObject_ARB);
		}

		/// <summary>
		/// Create a BufferObject name.
		/// </summary>
		/// <param name="ctx">
		/// A <see cref="GraphicsContext"/> used for creating this buffer object name.
		/// </param>
		/// <returns>
		/// It returns a valid object name for this BufferObject.
		/// </returns>
		/// <exception cref="ArgumentNullException">
		/// Exception thrown if <paramref name="ctx"/> is null.
		/// </exception>
		/// <exception cref="ArgumentException">
		/// Exception thrown if <paramref name="ctx"/> is not current on the calling thread.
		/// </exception>
		protected override uint CreateName(GraphicsContext ctx)
		{
			if (ctx == null)
				throw new ArgumentNullException("ctx");
			if (ctx.IsCurrent == false)
				throw new ArgumentException("not current");

			Debug.Assert(Gl.CurrentExtensions.VertexArrayObject_ARB);

			return (Gl.GenVertexArray());
		}

		/// <summary>
		/// Delete a BufferObject name.
		/// </summary>
		/// <param name="ctx">
		/// A <see cref="GraphicsContext"/> used for deleting this buffer object name.
		/// </param>
		/// <param name="name">
		/// A <see cref="UInt32"/> that specify the object name to delete.
		/// </param>
		/// <exception cref="ArgumentNullException">
		/// Exception thrown if <paramref name="ctx"/> is null.
		/// </exception>
		/// <exception cref="ArgumentException">
		/// Exception thrown if <paramref name="ctx"/> is not current on the calling thread.
		/// </exception>
		/// <exception cref="InvalidOperationException">
		/// Exception thrown if this BufferObject does not exist.
		/// </exception>
		protected override void DeleteName(GraphicsContext ctx, uint name)
		{
			CheckThisExistence(ctx);

			Debug.Assert(Gl.CurrentExtensions.VertexArrayObject_ARB);

			// Delete buffer object
			Gl.DeleteVertexArrays(name);
		}

		/// <summary>
		/// Actually create this BufferObject resources.
		/// </summary>
		/// <param name="ctx">
		/// A <see cref="GraphicsContext"/> used for allocating resources.
		/// </param>
		/// <exception cref="ArgumentNullException">
		/// Exception thrown if <paramref name="ctx"/> is null.
		/// </exception>
		/// <exception cref="ArgumentException">
		/// Exception thrown if <paramref name="ctx"/> is not current on the calling thread.
		/// </exception>
		/// <exception cref="InvalidOperationException">
		/// Exception thrown if this BufferObject has not client memory allocated and the hint is different from
		/// <see cref="BufferObjectHint.StaticCpuDraw"/> or <see cref="BufferObjectHint.DynamicCpuDraw"/>.
		/// </exception>
		/// <exception cref="InvalidOperationException">
		/// Exception thrown if this BufferObject is currently mapped.
		/// </exception>
		protected override void CreateObject(GraphicsContext ctx)
		{
			CheckCurrentContext(ctx);

			// Validate vertex array object
			Validate();

			// Create vertex arrays
			foreach (IVertexArray vertexArray in DrawArrays)
				vertexArray.Create(ctx);

			// Create element arrays
			foreach (Element element in DrawElements)
				element.Create(ctx);
		
			// Create feedback buffer
			if (_FeedbackBuffer != null)
				_FeedbackBuffer.Create(ctx);

			ctx.Bind(this);
		}

		/// <summary>
		/// Performs application-defined tasks associated with freeing, releasing, or resetting managed/unmanaged resources.
		/// </summary>
		/// <param name="disposing">
		/// A <see cref="Boolean"/> indicating whether this method is called by <see cref="Dispose()"/>. If it is false,
		/// this method is called by the finalizer.
		/// </param>
		protected override void Dispose(bool disposing)
		{
			// Base implementation
			base.Dispose(disposing);
			// Dispose related resources
			if (disposing) {
				// Unreference all collected array buffer objects
				foreach (VertexArray vertexArray in _VertexArrays.Values)
					vertexArray.Dispose();
				// Unreference all collected array indices
				foreach (Element vertexIndices in _Elements)
					vertexIndices.Dispose();
			}
		}

		#endregion

		#region IBindingResource Implementation

		/// <summary>
		/// Get the identifier of the binding point.
		/// </summary>
		int IBindingResource.BindingTarget
		{
			get
			{
				// Cannot lazy binding on textures if GL_ARB_vertex_array_object is not supported
				if (Gl.CurrentExtensions.VertexArrayObject_ARB == false)
					return (0);

				return (Gl.VERTEX_ARRAY_BINDING);
			}
		}

		/// <summary>
		/// Bind this IBindingResource.
		/// </summary>
		/// <param name="ctx">
		/// A <see cref="GraphicsContext"/> used for binding.
		/// </param>
		void IBindingResource.Bind(GraphicsContext ctx)
		{
			if (Gl.CurrentExtensions.VertexArrayObject_ARB)
				Gl.BindVertexArray(ObjectName);
		}

		/// <summary>
		/// Bind this IBindingResource.
		/// </summary>
		/// <param name="ctx">
		/// A <see cref="GraphicsContext"/> used for binding.
		/// </param>
		void IBindingResource.Unbind(GraphicsContext ctx)
		{
			if (Gl.CurrentExtensions.VertexArrayObject_ARB) {
				CheckThisExistence(ctx);
				Gl.BindVertexArray(InvalidObjectName);
			}
		}

		/// <summary>
		/// Check whether this IBindingResource is currently bound on the specified graphics context.
		/// </summary>
		/// <param name="ctx">
		/// A <see cref="GraphicsContext"/> used for querying the current binding state.
		/// </param>
		/// <returns>
		/// It returns a boolean value indicating whether this IBindingResource is currently bound on <paramref name="ctx"/>.
		/// </returns>
		bool IBindingResource.IsBound(GraphicsContext ctx)
		{
			int currentBufferObject;

			Debug.Assert(((IBindingResource)this).BindingTarget != 0);
			Gl.Get(((IBindingResource)this).BindingTarget, out currentBufferObject);

			return (currentBufferObject == (int)ObjectName);
		}

		#endregion
	}
}
