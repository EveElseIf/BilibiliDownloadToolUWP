Imports System
Imports System.IO
Imports NLog
Imports PInvoke
Imports Windows.ApplicationModel
Imports Windows.ApplicationModel.AppService
Imports Windows.Foundation.Collections
Imports Windows.Storage

Module Program
    Private _logger As Logger
    Sub Main(args As String())

        GlobalDiagnosticsContext.Set("LogPath", ApplicationData.Current.LocalFolder.Path + "\\")
        _logger = LogManager.GetCurrentClassLogger()

        Dim needConsole As Boolean? = ApplicationData.Current.LocalSettings.Values("NeedShowConsole")
        If needConsole = True Then
            ShowConsole()
        End If

        Dim connection = New AppServiceConnection() With
            {
            .PackageFamilyName = Package.Current.Id.FamilyName,
            .AppServiceName = "AppService"
        }
        AddHandler connection.RequestReceived, AddressOf Connection_RequestReceived
        AddHandler connection.ServiceClosed, AddressOf Connection_ServiceClosed
        Dim result = connection.OpenAsync().AsTask().Result
        If result <> AppServiceConnectionStatus.Success Then
            _logger.Fatal(result)
            Throw New Exception()
        End If
        _logger.Info(result)
        _logger.Info("辅助进程启动成功")
        While True
            Task.Delay(10000).Wait()
        End While
        _logger.Info("程序退出")
    End Sub
    Private Sub ShowConsole()
        If Not Kernel32.AllocConsole() Then
            _logger.Error($"辅助进程控制台分配失败，{Kernel32.GetLastError()}")
        End If
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
                .CreateNoWindow = True,
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
