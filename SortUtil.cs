using System;
using System.Collections.Generic;
using System.Text;

namespace IngicateWpf
{
    class SortUtil
    {
    //'IComparerインターフェイス
    //Implements IComparer

    //Private mOrder As SortOrder = SortOrder.Ascending  'ソート順(昇順・降順)
    //Private mKey As Integer = 0                        'ソート列

    //'ソート順(昇順・降順)プロパティ
    //Public Property Order() As SortOrder
    //    Get
    //        Return mOrder
    //    End Get
    //    Set(ByVal Value As SortOrder)
    //        mOrder = Value
    //    End Set
    //End Property

    //'ソート列プロパティ
    //Public Property Key() As Integer
    //    Get
    //        Return mKey
    //    End Get
    //    Set(ByVal Value As Integer)
    //        mKey = Value
    //    End Set
    //End Property

    //'比較結果を返す
    //void Compare(ByVal x As Object, ByVal y As Object) As Integer Implements IComparer.Compare
    //    Dim intRet As Integer

    //    Try
    //        '比較用リストアイテム格納変数
    //        Dim sx As ListViewItem = CType(x, ListViewItem)
    //        Dim sy As ListViewItem = CType(y, ListViewItem)

    //        '文字列を比較し、値を格納
    //        intRet = CompareString(sx.SubItems(mKey).Text, sy.SubItems(mKey).Text)

    //        '降順のときは結果を逆転
    //        If mOrder = SortOrder.Descending Then
    //            intRet = -intRet
    //        End If

    //    Catch ex As Exception

    //        Debug.WriteLine(ex.Message.ToString)

    //    End Try

    //    '結果を返す
    //    Return intRet

    //End Function

    //'文字列内の数値として認識できる部位を検査し、比較した値を返す
    //void CompareString(ByVal str1 As String, ByVal str2 As String) As Integer
    //    Dim chrNum() As Char = { "0", "1", "2", "3", "4", "5", "6", "7", "8", "9" }
    //    Dim i As Integer
    //    Dim intPos As Integer
    //    Dim intLength As Integer
    //    Dim intNext1 As Integer
    //    Dim intNext2 As Integer
    //    Dim intRet As Integer
    //    Dim idx As Integer
    //    Dim idx1 As Integer
    //    Dim idx2 As Integer
    //    Dim lngNum1 As Long = -1
    //    Dim lngNum2 As Long = -1

    //    'はじめの文字列から数値として認識できる部分を探す
    //    idx1 = str1.IndexOfAny(chrNum)

    //    '見つかった場合
    //    If idx1<> -1 Then
    //        '位置を格納
    //        intPos = idx1

    //        '長さの初期値
    //        intLength = 1

    //        '見つかった位置から文字列の終わりまで検査
    //        For i = idx1 + 1 To str1.Length - 1
    //            '数値として認識できるか検査
    //            idx = str1.Substring(i, 1).IndexOfAny(chrNum)

    //            '数値として認識できない場合は抜ける
    //            If idx = -1 Then Exit For

    //            '長さを格納
    //            intLength += 1
    //        Next

    //        '数値をLong値として格納
    //        lngNum1 = CType(str1.Substring(intPos, intLength), Long)

    //        '文字列の途中で終了した場合
    //        If intPos + intLength<> str1.Length Then
    //            'ループを抜けたポイントを格納
    //            intNext1 = intPos + intLength
    //        End If

    //    End If

    //    '次の文字列から数値として認識できる部分を探す
    //    idx2 = str2.IndexOfAny(chrNum)

    //    '見つかった場合
    //    If idx2 <> -1 Then
    //        '位置を格納
    //        intPos = idx2

    //        '長さの初期値
    //        intLength = 1

    //        '見つかった位置から文字列の終わりまで検査
    //        For i = idx2 + 1 To str2.Length - 1
    //            '数値として認識できるか検査
    //            idx = str2.Substring(i, 1).IndexOfAny(chrNum)

    //            '数値として認識できない場合は抜ける
    //            If idx = -1 Then Exit For

    //            '長さを格納
    //            intLength += 1
    //        Next

    //        '数値をLong値として格納
    //        lngNum2 = CType(str2.Substring(intPos, intLength), Long)

    //        '文字列の途中で終了した場合
    //        If intPos + intLength<> str2.Length Then
    //            'ループを抜けたポイントを格納
    //            intNext2 = intPos + intLength
    //        End If

    //    End If

    //    '数値が認識されなかったか、
    //    'または同じ位置ではない場合
    //    If idx1 = -1 Or idx2 = -1 Or (idx1<> idx2) Then
    //        '単純比較を行なう
    //        intRet = String.Compare(str1, str2)
    //    Else

    //        '文字列の先頭が数値として認識されたか、
    //        'または数値より前の文字列が同じ場合
    //        If (idx1 = 0 And idx2 = 0) Or _
    //            (str1.Substring(0, str1.Length - (str1.Length - idx1)) = str2.Substring(0, str2.Length - (str2.Length - idx2))) Then

    //            '数値として認識でき、ひとつめが大きい場合
    //            If lngNum1<> -1 And lngNum2 <> -1 And lngNum1 > lngNum2 Then
    //                '入れ替える値を格納
    //                intRet = 1
    //            ElseIf lngNum1 <> -1 And lngNum2 <> -1 And lngNum1 < lngNum2 Then
    //                '数値として認識でき、ひとつめが小さい場合は
    //                '入れ替えない値を格納
    //                intRet = -1
    //            ElseIf intNext1 <> 0 And intNext2 <> 0 Then
    //                '文字列の最後まで調べていない場合は再帰
    //                intRet = CompareString(str1.Substring(intNext1), str2.Substring(intNext2))
    //            Else
    //                '入れ替えない値を格納
    //                intRet = -1
    //            End If
    //        Else
    //            '数値より前の文字列が違う場合は単純比較
    //            intRet = String.Compare(str1, str2)
    //        End If
    //    End If

    //    '値を返す
    //    Return intRet

    //End Function

    }
}
