Imports Newtonsoft.Json
Imports Newtonsoft.Json.Linq

Module Module1

    Sub Main()

        Dim args() As String

        Dim Fs As IO.StreamReader
        Dim Fw As IO.StreamWriter

        Dim FileName1 As String
        Dim listData As String
        Dim strData() As String
        Dim timestamp As String
        Dim protocol As String
        Dim gasFee As String

        Dim transList As ArrayList = New ArrayList

        args = Split(Command(), " ")

        If args.Count < 1 Then
            Console.WriteLine("Error :less arguments")
            Exit Sub
        End If

        FileName1 = args(0)

        Fs = New IO.StreamReader(FileName1, System.Text.Encoding.Default)
        Fw = New IO.StreamWriter(FileName1 + ".txt", False, System.Text.Encoding.Default)

        listData = Fs.ReadLine
        listData = Fs.ReadLine

        Do While listData <> Nothing

            strData = listData.Split(";")
            '0:hash 1:timestamp 2:protocol 3:type 4:subtransaction 5:gas 6:contract

            timestamp = strData(1).Replace("""", "")
            protocol = strData(2).Replace("""", "")
            gasFee = strData(5).Replace("""", "")

            Dim subListData As String = strData(4).Remove(0, 1)
            subListData = subListData.Remove(subListData.Length - 1, 1)
            'subListData = subListData.Replace("[", "").Replace("]", "")

            Dim JsonObject As Object = JsonConvert.DeserializeObject(subListData)
            Dim jTokens As JEnumerable(Of JToken) = JsonObject.Children

            transList.Clear()

            For i = 0 To jTokens.Count - 1

                Dim tempData As Transact = New Transact
                Dim type As String = JsonObject(i)("type")

                tempData.Symbol = JsonObject(i)("symbol")

                If i = 0 Then

                    Select Case type

                        Case "incoming"
                            tempData.Incoming = JsonObject(i)("amount")
                            tempData.Outgoing = 0
                        Case "outgoing"
                            tempData.Incoming = 0
                            tempData.Outgoing = -JsonObject(i)("amount")
                        Case Else
                            Console.Write("error")

                    End Select

                    transList.Add(tempData)

                Else

                    For j = 0 To transList.Count - 1
                        Dim tempData1 As Transact = transList(j)

                        If tempData.Symbol = tempData1.Symbol Then

                            Select Case type

                                Case "incoming"
                                    tempData1.Incoming += JsonObject(i)("amount")
                                Case "outgoing"
                                    tempData1.Outgoing -= JsonObject(i)("amount")
                                Case Else
                                    Console.Write("error")

                            End Select

                            transList(j) = tempData1

                        Else

                            Select Case type

                                Case "incoming"
                                    tempData.Incoming = JsonObject(i)("amount")
                                    tempData.Outgoing = 0
                                Case "outgoing"
                                    tempData.Incoming = 0
                                    tempData.Outgoing = -JsonObject(i)("amount")
                                Case Else
                                    Console.Write("error")

                            End Select

                            transList.Add(tempData)

                        End If

                    Next

                End If

            Next

            For i = 0 To transList.Count - 1
                Dim tempData1 As Transact = transList(i)

                Dim wData As String
                wData = timestamp + "," + protocol + "," + tempData1.Symbol + ","

                If Not tempData1.Incoming = 0 Then
                    wData += "incoming" + "," + tempData1.Incoming.ToString + ","
                End If

                If Not tempData1.Outgoing = 0 Then
                    wData += "outgoing" + "," + tempData1.Outgoing.ToString + ","
                End If

                If i = 0 Then
                    wData += gasFee
                End If

                Fw.WriteLine(wData)
                Console.WriteLine(wData)

            Next




            listData = Fs.ReadLine

        Loop

        Fw.Close()
        Fs.Close()


    End Sub

End Module

Public Class Transact

    Implements System.IComparable

    Private _symbol As String
    Private _incoming As Double
    Private _outgoing As Double

    'Order
    Public Property Symbol() As String
        Get
            Return Me._symbol
        End Get
        Set(ByVal value As String)
            Me._symbol = value
        End Set
    End Property

    'incoming
    Public Property Incoming() As Single
        Get
            Return Me._incoming
        End Get
        Set(ByVal value As Single)
            Me._incoming = value
        End Set
    End Property

    'outcoimng
    Public Property Outgoing() As Single
        Get
            Return Me._outgoing
        End Get
        Set(ByVal value As Single)
            Me._outgoing = value
        End Set
    End Property


    Public Function CompareTo(other As Object) As Integer Implements IComparable.CompareTo

        If Not (Me.GetType() = other.GetType()) Then
            Throw New ArgumentException()
        End If

        Dim obj As Transact = CType(other, Transact)
        Return Me._symbol.CompareTo(obj._symbol)

        'Throw New NotImplementedException()

    End Function

End Class
