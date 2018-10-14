.head 3 +  ! Function: UpdateLieferTax
.head 4 -  Description: 
.head 4 -  Returns 
.head 4 +  Parameters 
.head 5 -  Number: p_nAR_NR
.data INHERITPROPS
0000: 0100
.enddata
.head 4 -  Static Variables 
.head 4 +  Local variables 
.head 5 -  Boolean: bOk
.head 5 -  String: sSelect
.head 4 +  Actions 
.head 5 -  Set sSelect = 'select * from ARTIKELLIEFERART
JOIN LIEFERARTIKELTAXIERUNG ON LT_LA_NR = AL_LA_NR AND LT_ART = 2
WHERE AL_STD = 1 AND AL_AR_NR = '  || NumberToStrX( p_nAR_NR )
.head 5 +  If Sql_FetchOne( hSql, sSelect )
.head 6 -  Set bOk = TRUE
.head 6 -  Set sSelect = 'delete FROM ARTIKELTAXIERUNG
where AT_ART = 2 AND AT_AR_NR = ' || NumberToStrX( p_nAR_NR )
.head 6 -  Set bOk = bOk AND Sql_Exec( hSql, sSelect )
.head 6 -  ! !
.head 6 -  ! wegen distinct
.head 6 -  Set dtGlobalTemp = DateOnly( SalDateCurrent(  ) )
.head 6 -  Set sSelect = 'insert into ARTIKELTAXIERUNG(AT_AR_NR,AT_ART,AT_TU_NR,AT_SB_NR,AT_UPDATE)
select DISTINCT AL_AR_NR,LT_ART,LT_TU_NR, :nUserNr, :dtGlobalTemp from ARTIKELLIEFERART
JOIN LIEFERARTIKELTAXIERUNG ON LT_LA_NR = AL_LA_NR AND LT_ART = 2
WHERE AL_STD = 1 AND AL_AR_NR = '  || NumberToStrX( p_nAR_NR )
.head 6 -  Set bOk = bOk AND Sql_Exec( hSqlSub, sSelect )
.head 6 -  ! !
.head 6 -  Set bOk = bOk AND Sql_Commit( hSqlSub )
.head 5 +  Else 
.head 6 -  Call MessageBox( 'Es sind keine Taxierungen vorhanden. Prьfen sie, ob
- ein als Standard markierter Lieferartikel mit dem Artikel verknьpft ist und
- der Lieferartikel Taxierungen hat', 'Taxierungen', MB_Ok )
.head 5 -  Return bOk
.head 3 +  Function: ДnderungenЬbernehmen ! th 20160929 eigene Parameter statt Window vars
.head 4 -  Description: 
.head 4 -  Returns 
.head 4 +  Parameters 
.head 5 -  Number: p_nAR_NR
.data INHERITPROPS
0000: 0100
.enddata
.head 5 -  String: p_sLA_SBLS1
.data INHERITPROPS
0000: 0100
.enddata
.head 5 -  String: p_sAR_SBLS
.data INHERITPROPS
0000: 0100
.enddata
.head 5 -  String: p_sStmts[*]
.data INHERITPROPS
0000: 0100
.enddata
.head 4 -  Static Variables 
.head 4 +  Local variables 
.head 5 -  Number: n
.head 5 -  FunctionalVar: oSql
.winattr class
.head 6 -  Class: cObjSqlHandle
.end
.head 4 +  Actions 
.head 5 +  If sLA_SBLS1 != '' AND p_nAR_NR != NUMBER_Null
.head 6 -  Call MergeBlsErstellen( p_nAR_NR, p_sLA_SBLS1, p_sAR_SBLS ) 
.head 5 +  While p_sStmts[n] != ''
.head 6 -  Call Sql_ExecAndCommit( oSql.Sql (), p_sStmts[n] )
.head 6 -  Set n = n + 1
.head 5 -  ! Flag zurьcksetzen
.head 5 -  Call Sql_ExecAndCommit( oSql.Sql (), 'UPDATE artikel SET ar_zuordnungneu = 0 WHERE ar_nr = '|| NumberToStrX( p_nAR_NR ) )
.head 5 -  Set tblArtikelDatenьbertrag.sTableName = 'artlog'
.head 5 -  Call tblArtikelDatenьbertrag.InsertRow ()
.head 5 -  Call SalEndDialog(hWndForm,TRUE)
.head 5 +  If NOT bAutomatik
.head 6 -  ! Tabelle neu laden
.head 6 -  Call SalSendMsg( tblArtikel, AM_POPULATE, 0, 0)