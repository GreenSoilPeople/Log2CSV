Imports System.IO
Imports System.Text.RegularExpressions

Module Module1
    Private InputDir As String
    Private OutputFilePath As String
    Private CurrentChassis As String
    Sub Main()

        Dim args As String() = Environment.GetCommandLineArgs

        If args.Length = 1 Then
            Console.WriteLine("Arguments missing")
            Console.WriteLine("Usage: log2csv <input_folder> <output_file>")
            Exit Sub
        End If

        If args.Length >= 2 Then
            InputDir = args(1)
            OutputFilePath = args(2)

            If Not Directory.Exists(InputDir) Then
                Console.WriteLine("Input folder invalid")
                Exit Sub
            End If

        End If



        Dim outputLines As New List(Of OutputLine)

        Dim r1 As Regex = New Regex("sn(\d{2})-c(\d{2})")
        Dim r2 As Regex = New Regex("\*\* Group (\d+)")

        Dim r3 As Regex = New Regex("Group (\d+) Result")
        Dim r4 As Regex = New Regex("HPL LINPACK:\s+(.+)")
        Dim r5 As Regex = New Regex("You achieved ([0-9\.\%]+) of your peak ([0-9\.]+ \w+\/s)")


        For Each file As String In Directory.GetFiles(InputDir, "*.log")

            CurrentChassis = Regex.Match(Path.GetFileName(file), "chassis(\d+)").Groups(1).Value

            Dim lines As String() = IO.File.ReadAllLines(file)

            outputLines.Add(New OutputLine With {.Group = "0", .Chassis = CurrentChassis})

            For i As Integer = 0 To lines.Count - 3
                Dim m1 As Match = r1.Match(lines(i))

                If Not String.IsNullOrWhiteSpace(m1.Value) Then

                    Dim m2 As Match = r2.Match(lines(i + 1))

                    If Not String.IsNullOrWhiteSpace(m2.Value) Then

                        AddOutput(outputLines, New OutputLine With {.Chassis = m1.Groups(1).Value,
                                                                .Cartridge = m1.Groups(2).Value,
                                                                .Group = m2.Groups(1).Value})


                    End If

                Else
                    Dim m3 As Match = r3.Match(lines(i))
                    Dim m4 As Match = r4.Match(lines(i + 1))
                    Dim m5 As Match = r5.Match(lines(i + 2))

                    If Not String.IsNullOrWhiteSpace(m3.Value) AndAlso Not String.IsNullOrWhiteSpace(m4.Value) AndAlso Not String.IsNullOrWhiteSpace(m5.Value) Then
                        'Console.WriteLine(lines(i))
                        With outputLines(outputLines.FindIndex(Function(x) x.Group = m3.Groups(1).Value And x.Chassis = CurrentChassis))
                            .Result = m4.Groups(1).Value
                            .Percentage = m5.Groups(1).Value
                            .Peak = m5.Groups(2).Value
                        End With
                    End If

                End If
            Next
        Next

        'For Each o As OutputLine In outputLines
        '    Console.WriteLine($"chassis: {o.Chassis}# cartridge: {o.Cartridge}# group: {o.Group}# result: {o.Result}# percentage: {o.Percentage}# peak: {o.Peak}")
        '    Console.WriteLine()
        'Next


        WriteCSV(outputLines)
        Console.WriteLine("Done")
        Console.ReadLine()
    End Sub

    Sub AddOutput(ByRef list As List(Of OutputLine), outline As OutputLine)
        If Not list.Exists(Function(x) x.Cartridge = outline.Cartridge And x.Chassis = outline.Chassis) Then list.Add(outline)
        'If Not list.Contains(outline) Then list.Add(outline)
    End Sub

    Sub WriteCSV(output As List(Of OutputLine))

        If File.Exists(OutputFilePath) Then File.Delete(OutputFilePath)

        Dim sr As New StreamWriter(OutputFilePath, True)
        sr.WriteLine($"Chassis{vbTab}Cartridge{vbTab}Group{vbTab}Result{vbTab}Percentage{vbTab}Peak")
        For Each o As OutputLine In output
            sr.WriteLine($"{o.Chassis}{vbTab}{o.Cartridge}{vbTab}{o.Group}{vbTab}{o.Result}{vbTab}{o.Percentage}{vbTab}{o.Peak}")
        Next
        sr.Close()
    End Sub
End Module

Class OutputLine
    Property Chassis As String
    Property Cartridge As String
    Property Group As String
    Property Result As String
    Property Percentage As String
    Property Peak As String
End Class