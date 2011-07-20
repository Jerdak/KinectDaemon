With CreateObject("MSXML2.XMLHTTP")
  .open "GET", "http://automation.whatismyip.com/n09230945.asp", False
  .send
  WScript.Echo .responseText
End With