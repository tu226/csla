Imports System.Reflection
Imports System.ComponentModel

Namespace Data

  Public Module DataMapper

#Region " Map From IDictionary "

    ''' <summary>
    ''' Copies values from the source into the
    ''' properties of the target.
    ''' </summary>
    ''' <param name="source">A name/value dictionary containing the source values.</param>
    ''' <param name="target">An object with properties to be set from the dictionary.</param>
    ''' <remarks>
    ''' The key names in the dictionary must match the property names on the target
    ''' object. Target properties may not be readonly or indexed.
    ''' </remarks>
    Public Sub Map(ByVal source As System.Collections.IDictionary, ByVal target As Object)

      Map(source, target, False)

    End Sub

    ''' <summary>
    ''' Copies values from the source into the
    ''' properties of the target.
    ''' </summary>
    ''' <param name="source">A name/value dictionary containing the source values.</param>
    ''' <param name="target">An object with properties to be set from the dictionary.</param>
    ''' <param name="ignoreList">A list of property names to ignore. 
    ''' These properties will not be set on the target object.</param>
    ''' <remarks>
    ''' The key names in the dictionary must match the property names on the target
    ''' object. Target properties may not be readonly or indexed.
    ''' </remarks>
    Public Sub Map(ByVal source As System.Collections.IDictionary, ByVal target As Object, _
      ByVal ParamArray ignoreList() As String)

      Map(source, target, False, ignoreList)

    End Sub

    ''' <summary>
    ''' Copies values from the source into the
    ''' properties of the target.
    ''' </summary>
    ''' <param name="source">A name/value dictionary containing the source values.</param>
    ''' <param name="target">An object with properties to be set from the dictionary.</param>
    ''' <param name="ignoreList">A list of property names to ignore. 
    ''' These properties will not be set on the target object.</param>
    ''' <param name="supressExceptions">If True, any exceptions will be supressed.</param>
    ''' <remarks>
    ''' The key names in the dictionary must match the property names on the target
    ''' object. Target properties may not be readonly or indexed.
    ''' </remarks>
    Public Sub Map(ByVal source As System.Collections.IDictionary, ByVal target As Object, _
      ByVal supressExceptions As Boolean, ByVal ParamArray ignoreList() As String)

      Dim ignore As New List(Of String)(ignoreList)
      Dim targetType As Type = target.GetType
      For Each propertyName As String In source.Keys
        If Not ignore.Contains(propertyName) Then
          Try
            SetValue(target, propertyName, source.Item(propertyName))

          Catch
            If Not supressExceptions Then
              Throw New ArgumentException( _
                String.Format("{0} ({1})", My.Resources.PropertyCopyFailed, propertyName))
            End If
          End Try
        End If
      Next

    End Sub

#End Region

#Region " Map from Object "

    ''' <summary>
    ''' Copies values from the source into the
    ''' properties of the target.
    ''' </summary>
    ''' <param name="source">An object containing the source values.</param>
    ''' <param name="target">An object with properties to be set from the dictionary.</param>
    ''' <remarks>
    ''' The property names and types of the source object must match the property names and types
    ''' on the target object. Source properties may not be indexed. 
    ''' Target properties may not be readonly or indexed.
    ''' </remarks>
    Public Sub Map(ByVal source As Object, ByVal target As Object)

      Map(source, target, False)

    End Sub

    ''' <summary>
    ''' Copies values from the source into the
    ''' properties of the target.
    ''' </summary>
    ''' <param name="source">An object containing the source values.</param>
    ''' <param name="target">An object with properties to be set from the dictionary.</param>
    ''' <param name="ignoreList">A list of property names to ignore. 
    ''' These properties will not be set on the target object.</param>
    ''' <remarks>
    ''' The property names and types of the source object must match the property names and types
    ''' on the target object. Source properties may not be indexed. 
    ''' Target properties may not be readonly or indexed.
    ''' </remarks>
    Public Sub Map(ByVal source As Object, ByVal target As Object, _
      ByVal ParamArray ignoreList() As String)

      Map(source, target, False, ignoreList)

    End Sub

    ''' <summary>
    ''' Copies values from the source into the
    ''' properties of the target.
    ''' </summary>
    ''' <param name="source">An object containing the source values.</param>
    ''' <param name="target">An object with properties to be set from the dictionary.</param>
    ''' <param name="ignoreList">A list of property names to ignore. 
    ''' These properties will not be set on the target object.</param>
    ''' <param name="supressExceptions">If True, any exceptions will be supressed.</param>
    ''' <remarks>
    ''' <para>
    ''' The property names and types of the source object must match the property names and types
    ''' on the target object. Source properties may not be indexed. 
    ''' Target properties may not be readonly or indexed.
    ''' </para><para>
    ''' Properties to copy are determined based on the source object. Any properties
    ''' on the source object marked with the <see cref="BrowsableAttribute"/> equal
    ''' to false are ignored.
    ''' </para>
    ''' </remarks>
    Public Sub Map(ByVal source As Object, ByVal target As Object, _
      ByVal supressExceptions As Boolean, ByVal ParamArray ignoreList() As String)

      Dim ignore As New List(Of String)(ignoreList)
      Dim sourceType As Type = source.GetType
      Dim sourceProperties As PropertyInfo() = GetSourceProperties(sourceType)
      Dim targetType As Type = target.GetType
      For Each sourceProperty As PropertyInfo In sourceProperties
        Dim propertyName As String = sourceProperty.Name
        If Not ignore.Contains(propertyName) Then
          Try
            Dim propertyInfo As PropertyInfo
            propertyInfo = sourceType.GetProperty(propertyName)
            SetValue(target, propertyName, propertyInfo.GetValue(source, Nothing))

          Catch
            If Not supressExceptions Then
              Throw New ArgumentException( _
                String.Format("{0} ({1})", My.Resources.PropertyCopyFailed, propertyName))
            End If
          End Try
        End If
      Next

    End Sub

    Private Function GetSourceProperties(ByVal sourceType As Type) As PropertyInfo()

      Dim result As New Generic.List(Of PropertyInfo)
      Dim props As PropertyDescriptorCollection = _
        TypeDescriptor.GetProperties(sourceType)
      For Each item As PropertyDescriptor In props
        If item.IsBrowsable Then
          result.Add(sourceType.GetProperty(item.Name))
        End If
      Next
      Return result.ToArray

    End Function

#End Region

    Private Sub SetValue(ByVal target As Object, ByVal propertyName As String, ByVal value As Object)

      Dim propertyInfo As PropertyInfo = _
        target.GetType.GetProperty(propertyName)
      Dim pType As Type = propertyInfo.PropertyType
      If value Is Nothing Then
        propertyInfo.SetValue(target, value, Nothing)

      Else
        If pType.Equals(value.GetType) Then
          ' types match, just copy value
          propertyInfo.SetValue(target, value, Nothing)

        Else
          ' types don't match, try to coerce types
          If pType.Equals(GetType(Guid)) Then
            propertyInfo.SetValue(target, New Guid(value.ToString), Nothing)

          Else
            propertyInfo.SetValue(target, _
              Convert.ChangeType(value, pType), Nothing)
          End If
        End If
      End If

    End Sub

  End Module

End Namespace
