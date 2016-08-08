Imports System.Runtime.InteropServices
Imports System.Windows.Forms

Namespace UserControls

    Public Class ScrollableLabel
        Inherits System.Windows.Forms.TextBox
        <DllImport("user32", EntryPoint:="HideCaret")>
        Private Shared Function HideCaret(hWnd As IntPtr) As Boolean
        End Function

        <DllImport("user32", EntryPoint:="ShowCaret")>
        Private Shared Function ShowCaret(hWnd As IntPtr) As Boolean
        End Function

        Public Sub New()
            MyBase.New()

            Me.TabStop = False
            Me.SetStyle(ControlStyles.Selectable, False)
            Me.Cursor = Cursors.[Default]
            Me.[ReadOnly] = True
            Me.ShortcutsEnabled = False
            Me.HideSelection = True
            AddHandler Me.GotFocus, AddressOf TextBoxLabel_GotFocus
            AddHandler Me.MouseMove, AddressOf TextBoxLabel_MouseMove

        End Sub

        Private Sub TextBoxLabel_GotFocus(sender As [Object], e As System.EventArgs)
            If ShowCaret(DirectCast(sender, TextBox).Handle) Then
                HideCaret(DirectCast(sender, TextBox).Handle)
            End If
        End Sub

        Private Sub TextBoxLabel_MouseMove(sender As [Object], e As MouseEventArgs)
            If DirectCast(sender, TextBox).SelectedText.Length > 0 Then
                DirectCast(sender, TextBox).SelectionLength = 0
            End If
        End Sub
    End Class
End Namespace
