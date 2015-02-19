Imports System.Data

Class Snow

    Sub getCurrentMonthData()
        'http://www1.ncdc.noaa.gov/pub/data/snowmonitoring/fema/02-2014-dlysnfl.txt

        Dim conn As New MySql.Data.MySqlClient.MySqlConnection(Common.mySqlConnectionString)
        conn.Open()

        Dim todaydate As Date = Now

        Dim txt As String = Common.GetHtmlPage("http://www1.ncdc.noaa.gov/pub/data/snowmonitoring/fema/" & Common.zerofill(todaydate.Month) & "-" & todaydate.Year & "-dlysnfl.txt")
        txt = txt.Substring(txt.IndexOf("ALLENTOWN"))
        txt = txt.Substring(0, txt.IndexOf(vbLf))
        txt = txt.Replace("  ", " ").Replace("  ", " ").Replace("  ", " ").Replace("  ", " ").Replace("  ", " ").Replace("  ", " ")
        Dim counter As String = 1
        For Each s As String In txt.Split(" ")
            If counter > 4 Then
                Dim sql As String = "insert into currentMonthWeather values(18000," & todaydate.Month & "," & counter - 4 & "," & s & ",'" & Common.mySqlDateFormat(todaydate) & "')"
                Try
                    Dim comm2 As New MySql.Data.MySqlClient.MySqlCommand(sql, conn)
                    comm2.ExecuteNonQuery()
                Catch ex As Exception
                    Response.Write(ex.Message & "<hr/" & sql)
                End Try
            End If
            counter += 1
        Next
        conn.Close()

    End Sub

    Sub getHistoricalData()
        Dim conn As New MySql.Data.MySqlClient.MySqlConnection(Common.mySqlConnectionString)
        conn.Open()

        Dim sqlDel As String = "delete from historicalweather where 1=1"
        Dim comm As New MySql.Data.MySqlClient.MySqlCommand(sqlDel, conn)
        comm.ExecuteNonQuery()

        'http://weather-warehouse.com/WeatherHistory/PastWeatherData_AllentownLehighValleyIntlArpt_LehighValley_PA_February.html
        For inti As Integer = 1 To 12
            Dim html As String = Common.GetHtmlPage("http://weather-warehouse.com/WeatherHistory/PastWeatherData_AllentownLehighValleyIntlArpt_LehighValley_PA_" & Common.GetMonth(inti) & ".html")
            html = html.Substring(html.IndexOf("stripeMe"))
            html = html.Substring(0, html.IndexOf("</table>"))


            Dim firstRow As Boolean = True
            Dim dt As New DataTable
            While html.IndexOf("<tr>") >= 0
                If firstRow Then
                    While html.IndexOf("<th") > 0
                        Dim rowName As String = String.Empty
                        html = html.Substring(html.IndexOf("<th"))
                        html = html.Substring(html.IndexOf("<span id='") + 10)
                        rowName = html.Substring(0, html.IndexOf("'"))
                        dt.Columns.Add(rowName)
                        html = html.Substring(html.IndexOf("</th"))
                    End While
                    firstRow = False
                Else
                    Dim intj As Integer = 0
                    Dim dr As DataRow = dt.NewRow
                    While html.IndexOf("<td") > 0
                        html = html.Substring(html.IndexOf("<td>") + 4)
                        Dim data As String = html.Substring(0, html.IndexOf("</td>"))
                        If intj < dt.Columns.Count Then
                            If data = "NR" Then
                                data = -100
                            End If
                            dr(intj) = data
                        End If
                        intj += 1
                        If html.IndexOf("<tr") < html.IndexOf("<td") Then
                            Exit While
                        End If
                    End While
                    dt.Rows.Add(dr)
                End If
                html = html.Substring(html.IndexOf("</tr"))

            End While

            For Each dr As DataRow In dt.Rows
                For Each dc As DataColumn In dt.Columns
                    Try
                        If dr(dc.ColumnName) = "" Then
                            dr(dc.ColumnName) = -100
                        End If
                    Catch ex As Exception
                        dr(dc.ColumnName) = -100
                    End Try

                Next
            Next

            Dim todaydate As Date = Now
            For Each dr As DataRow In dt.Rows
                Dim seasonYear As String = dr("year")
                If (inti < 7) Then
                    seasonYear = Convert.ToInt32(dr("year")) - 1
                Else

                End If
                Dim sql As String = "insert into historicalWeather values(18000," & inti & "," & seasonYear & "," & dr("year") & "," & dr("lotemp") & "," & dr("hitemp") & "," & dr("avgmin") & "," & dr("avgmax") & "," & dr("mean") & "," & dr("precip") & "," & dr("snow") & ",'" & Common.mySqlDateFormat(todaydate) & "')"
                Try
                    Dim comm2 As New MySql.Data.MySqlClient.MySqlCommand(sql, conn)
                    comm2.ExecuteNonQuery()
                Catch ex As Exception
                    Response.Write(ex.Message & "<hr/>" & sql)
                End Try
            Next

        Next
        conn.Close()

    End Sub


    Sub weatherAPI()
        Dim conn As New MySql.Data.MySqlClient.MySqlConnection(Common.mySqlConnectionString)
        conn.Open()
        Dim xmld As System.Xml.XmlDocument
        Dim nodelist As XmlNodeList
        Dim node As XmlNode

        xmld = New XmlDocument()
        xmld.Load(Common.GetHtmlPageStream("http://api.wunderground.com/api/GETYOUROWNAPIKEY/alerts/conditions/forecast10day/history/yesterday/q/KABE.xml"))

        Dim sqlCon As String = "insert into weatherConditions values('" & Common.esc(xmld.SelectSingleNode("/response/current_observation/icon_url").InnerText) & "','" & _
        Common.esc(xmld.SelectSingleNode("/response/current_observation/temperature_string").InnerText) & "','" & _
        Common.esc(xmld.SelectSingleNode("/response/current_observation/weather").InnerText) & "','" & _
        Common.esc(xmld.SelectSingleNode("/response/current_observation/observation_time").InnerText) & "','" & _
        Common.esc(xmld.SelectSingleNode("/response/current_observation/feelslike_string").InnerText) & "','" & _
        Common.esc(xmld.SelectSingleNode("/response/current_observation/relative_humidity").InnerText) & "','" & _
        Common.esc(xmld.SelectSingleNode("/response/current_observation/wind_string").InnerText) & "','" & _
        Common.esc(xmld.SelectSingleNode("/response/current_observation/precip_today_string").InnerText) & "','" & _
        Common.esc(xmld.SelectSingleNode("/response/current_observation/forecast_url").InnerText) & "','" & _
        Common.esc(xmld.SelectSingleNode("/response/current_observation/history_url").InnerText) & "','" & _
        Common.mySqlDateFormat(Now) & "')"
        Try
            Dim comm2 As New MySql.Data.MySqlClient.MySqlCommand(sqlCon, conn)
            comm2.ExecuteNonQuery()
        Catch ex As Exception
            Response.Write(ex.Message & "<hr/><pre>" & sqlCon & "</pre><hr/>")
        End Try

        For Each n As XmlNode In xmld.GetElementsByTagName("alert")
            Dim sqlAlert As String = "insert into weatherAlerts values('" & Common.esc(n.SelectSingleNode("description").InnerText) & "','" & _
                Common.esc(n.SelectSingleNode("date").InnerText) & "','" & _
                Common.esc(n.SelectSingleNode("expires").InnerText) & "','" & _
               Common.esc(n.SelectSingleNode("message").InnerText) & "','" & _
                Common.mySqlDateFormat(Now) & "')"
            Try
                Dim comm2 As New MySql.Data.MySqlClient.MySqlCommand(sqlAlert, conn)
                comm2.ExecuteNonQuery()
            Catch ex As Exception
                Response.Write(ex.Message & "<hr/><pre>" & sqlAlert & "</pre><hr/>")
            End Try

        Next

        Dim currDate As Date = Now
        Dim am As Boolean = True

        For Each n As XmlNode In xmld.GetElementsByTagName("forecastday")
            Try
                Dim sqlF As String = "insert into forecastDay values(" & currDate.Month & "," & _
                currDate.Day & "," & _
                currDate.Year & "," & _
                Common.esc(n.SelectSingleNode("period").InnerText) & ",'"
                If am Then
                    sqlF &= "am','"
                Else
                    sqlF &= "pm','"
                End If
                sqlF &= Common.esc(n.SelectSingleNode("icon_url").InnerText) & "','" & _
                Common.esc(n.SelectSingleNode("title").InnerText) & "','" & _
               Common.esc(n.SelectSingleNode("fcttext").InnerText) & "','" & _
                Common.mySqlDateFormat(Now) & "')"
                Try
                    Dim comm2 As New MySql.Data.MySqlClient.MySqlCommand(sqlF, conn)
                    comm2.ExecuteNonQuery()
                Catch ex As Exception
                    Response.Write(ex.Message & "<hr/><pre>" & sqlF & "</pre><hr/>")
                End Try


                If am Then
                    am = False
                Else
                    currDate = currDate.AddDays(1)
                    am = True
                End If

            Catch ex As Exception

            End Try
        Next

        xmld = Nothing
        conn.Close()
    End Sub


End Class
