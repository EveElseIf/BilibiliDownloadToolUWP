Imports System
Imports System.IO
Imports Windows.ApplicationModel
Imports Windows.ApplicationModel.AppService
Imports Windows.Foundation.Collections

Module Program
    Sub Main(args As String())
        Dim connection = New AppServiceConnection() With
            {
            .PackageFamilyName = Package.Current.Id.FamilyName,
            .AppServiceName = "AppService"
        }
        AddHandler connection.RequestReceived, AddressOf Connection_RequestReceived
        AddHandler connection.ServiceClosed, AddressOf Connection_ServiceClosed
        Dim result = connection.OpenAsync().AsTask().Result
        If result <> AppServiceConnectionStatus.Success Then Throw New Exception()
        Console.In.ReadLineAsync().Wait()
    End Sub

    Private Sub Connection_ServiceClosed(sender As AppServiceConnection, args As AppServiceClosedEventArgs)
        Environment.Exit(0)
    End Sub

    Private Async Sub Connection_RequestReceived(sender As AppServiceConnection, args As AppServiceRequestReceivedEventArgs)
        Dim def = args.GetDeferral()
        Dim key = args.Request.Message.First().Key
        If key = "ffmpeg" Then
            Dim values = args.Request.Message.First().Value.ToString().Split(vbLf)
            Dim p As New Process With {
                .StartInfo = New ProcessStartInfo("ffmpeg.exe", values(0)) With {
                .RedirectStandardOutput = True}
            }
            AddHandler p.OutputDataReceived, Function(s, e)
                                                 Console.WriteLine(e.Data)
                                                 Return 0
                                             End Function
            p.Start()
            p.WaitForExit()
            Try
                Directory.Delete(values(1), True)
            Catch ex As Exception

            End Try
        End If
        Await args.Request.SendResponseAsync(New ValueSet() From {{"ok", ""}})
        def.Complete()
    End Sub
End Module
