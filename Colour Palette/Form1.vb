Imports System.Collections.ObjectModel
Imports System.Numerics
Imports System.IO
Public Class Form1
    Public Structure paletteData
        Public brush As Brush
        Public clipBoardData As Object
        Public size As Size
        Public location As Vector2
        Public colour As Color
        Public Function asString()
            Return clipBoardData.ToString & ";" & size.Width & ";" & size.Height & ";" & location.X & ";" & location.Y & ";" & colour.A & ";" & colour.R & ";" & colour.G & ";" & colour.B
        End Function
        Public Sub fromString(inputStr As String)
            Dim inputs() As String = inputStr.Split(";")
            clipBoardData = inputs(0)
            size.Width = inputs(1)
            size.Height = inputs(2)
            location.X = inputs(3)
            location.Y = inputs(4)
            colour = Color.FromArgb(inputs(5), inputs(6), inputs(7), inputs(8))
            brush = New SolidBrush(colour)
        End Sub
    End Structure
    Dim paletteColour As New Collection(Of paletteData)

    Private Sub Form1_Load(sender As Object, e As EventArgs) Handles MyBase.Load
        BackgroundImage = New Bitmap(500, 500)
        Dim loadFromFile As MsgBoxResult = MsgBox("Load from file?", MsgBoxStyle.YesNo, "Colour Palette")
        If loadFromFile = MsgBoxResult.Yes Then
            If Not Directory.Exists(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData) & "\ColourPalettes\") Then
                Directory.CreateDirectory(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData) & "\ColourPalettes\")
            End If
            Dim loadfile As String = ""
                With OpenFileDialog1
                    .AddExtension = True
                    .CheckFileExists = True
                    .DefaultExt = ".pal"
                    .InitialDirectory = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData) & "\ColourPalettes\"
                    .Multiselect = False
                    .FileName = "save.pal"
                    .ShowDialog()
                    loadfile = .FileName
                End With
                Text = Path.GetFileNameWithoutExtension(loadfile)
                Dim fileContents() As String = File.ReadAllLines(loadfile)
                For Each line In fileContents
                    Dim newSpot As paletteData
                    newSpot.fromString(line)
                    paletteColour.Add(newSpot)
                Next
            End If

            Timer1.Interval = 1   'This just calls the draw function 1 millisecond after loading, as it for some reason cant draw during the loading stage
        Timer1.Start()
    End Sub
    Private Sub Form1_Closing(sender As Object, e As FormClosingEventArgs) Handles MyBase.Closing
        Dim loadFromFile As MsgBoxResult = MsgBox("Save to file?", MsgBoxStyle.YesNoCancel, "Colour Palette")
        If loadFromFile = MsgBoxResult.Yes Then
            If Not Directory.Exists(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData) & "\ColourPalettes\") Then
                Directory.CreateDirectory(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData) & "\ColourPalettes\")
            End If
            Dim saveDirectory As String = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData) & "\ColourPalettes\"
            Dim savefile As String = ""
            Dim firstItem As Boolean = True
            Dim fileContents As String = ""
            For Each spot In paletteColour
                If Not firstItem Then
                    fileContents &= vbCrLf
                End If
                firstItem = False
                fileContents &= spot.asString
            Next
            With SaveFileDialog1
                .AddExtension = True
                .DefaultExt = ".pal"
                .FileName = "save.pal"
                .InitialDirectory = saveDirectory
                .ShowDialog()
                savefile = .FileName
            End With
            File.WriteAllText(savefile, fileContents)
        ElseIf loadFromFile = MsgBoxResult.Cancel Then
            e.Cancel = True
        End If
    End Sub
    Private Sub Form1_Click(sender As Object, e As MouseEventArgs) Handles MyBase.Click
        Dim containsItem As Boolean = False
        Dim intersectsEllipse As Boolean = False
        For Each spot In paletteColour.ToList()
            containsItem = True
            If InEllipse(spot.location, spot.size, New Point(e.X, e.Y)) Then
                intersectsEllipse = True
                If e.Button = MouseButtons.Left Then
                    My.Computer.Clipboard.SetText(spot.clipBoardData)
                ElseIf e.Button = MouseButtons.Right Then
                    paletteColour.Remove(spot)
                End If
            End If
        Next
        If (Not intersectsEllipse) And e.Button = MouseButtons.Left Then
            containsItem = True
            AddSpot(e)
        ElseIf (Not containsItem) And e.Button = MouseButtons.Left Then
            AddSpot(e)
        End If
        FormResized()
    End Sub
    Public Sub FormResized() Handles Me.Resize
        Draw()
    End Sub
    Public Sub Draw()
        Dim gfx As Graphics = Me.CreateGraphics
        gfx.Clear(Color.White)
        For Each spot In paletteColour
            If spot.colour.GetBrightness() > 0.8 Then
                Const borderStroke As Integer = 2
                gfx.FillEllipse(Brushes.Black, New Rectangle(spot.location.X - borderStroke, spot.location.Y - borderStroke, spot.size.Width + (2 * borderStroke), spot.size.Height + (2 * borderStroke)))
            End If
            gfx.FillEllipse(spot.brush, New Rectangle(spot.location.X, spot.location.Y, spot.size.Width, spot.size.Height))
        Next
        gfx.Dispose()
    End Sub
    Public Function AddSpot(e As MouseEventArgs) As Boolean
        If My.Computer.Clipboard.ContainsText Then
            Dim allowedChars As Char() = {"a", "b", "c", "d", "e", "f", "1", "2", "3", "4", "5", "6", "7", "8", "9", "0"}
            Dim validHexCode As Boolean = True
            Dim data As String = My.Computer.Clipboard.GetText
            If data.Length = 6 Then
                For Each letter In data
                    If Not allowedChars.Contains(LCase(letter)) Then
                        validHexCode = False
                    End If
                Next
                data = LCase(data)
                data = "#" & data
            ElseIf data.Length = 7 Then
                For letterIndex = 1 To 6
                    If Not allowedChars.Contains(LCase(data(letterIndex))) Then
                        validHexCode = False
                    End If
                Next
                If data(0) <> "#" Then
                    validHexCode = False
                End If
                data = LCase(data)
            Else
                validHexCode = False
            End If

            If validHexCode Then
                Dim newSpot As paletteData
                'Dim rand As New Random
                'newSpot.size = New Size(rand.Next(50, 70), rand.Next(50, 70)) 'Random
                newSpot.size = New Size(30, 30) 'Predictable sizes
                newSpot.location = New Vector2(e.X - newSpot.size.Width / 2, e.Y - newSpot.size.Height / 2)
                newSpot.clipBoardData = data
                newSpot.brush = New SolidBrush(hexToColor(Strings.Right(data, 6)))
                newSpot.colour = hexToColor(Strings.Right(data, 6))
                paletteColour.Add(newSpot)
            Else
                MsgBox("Hex value in the wrong form")
            End If
        End If
    End Function
    Private Function InEllipse(centreLocation As Vector2, size As Size, click As Point) As Boolean
        Dim x As Decimal = click.X
        Dim y As Decimal = click.Y
        Dim a As Decimal = size.Width / 2
        Dim b As Decimal = size.Height / 2
        Dim x1 As Decimal = centreLocation.X
        Dim y1 As Decimal = centreLocation.Y
        Dim isInEllipse As Decimal = ((x - x1 - a) ^ 2) * ((1 / a) ^ 2) + ((y - y1 - b) ^ 2) * ((1 / b) ^ 2)
        '\left(x-x_{1}-a\right)^{2}\left(\frac{1}{a}\right)^{2}+\left(y-y_{1}-b\right)^{2}\left(\frac{1}{b}\right)^{2}<1
        '  ^ LaTeX for the above function where a and b are width and height, x1 and y1 are ellipse locaiton and x and y are mouse locations
        If isInEllipse <= 1 Then
            Return True
        Else
            Return False
        End If
    End Function
    Private Function hexToColor(hex As String) As Color
        Dim r, g, b As Byte
        Dim rHex As String = hex.Substring(0, 2)
        Dim gHex As String = hex.Substring(2, 2)
        Dim bHex As String = hex.Substring(4, 2)
        r = hexToDecimal(rHex)
        g = hexToDecimal(gHex)
        b = hexToDecimal(bHex)
        Return Color.FromArgb(255, r, g, b)
    End Function
    Private Function hexToDecimal(hex As String) As Decimal
        Return CInt("&H" & hex)
    End Function

    Private Sub Timer1_Tick(sender As Object, e As EventArgs) Handles Timer1.Tick
        Draw()
        Timer1.Stop()
    End Sub
End Class
