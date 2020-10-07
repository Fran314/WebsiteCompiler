# WebsiteCompiler - con supporto per pagina multilingua e upload a Poisson

(Non so spiegare benissimo via testo come funziona questo software, a parole mi viene decisamente meglio. Se avete dubbi o perplessità contattatemi pure)

WebsiteCompiler è un software per Windows che si pone l'obiettivo di risolvere i seguenti 3 problemi:
### 1) Unificare l'informazione di blocchi ripetuti sempre uguali nelle varie pagine
Nel mio sito, ad esempio, è presente un header ripetuto in tutte le pagine. L'header però è sempre lo stesso. Ciò vuol dire che se devo fare un cambiamento all'header, devo modificarlo in 7 file diversi, quando idealmente dovrei poterlo modificare una volta sola (essendo lui uguale in tutte e 7 le pagine).

### 2) Unificare l'informazione di una stessa pagina in diverse lingue:
I file della pagina in italiano e della pagina in inglese sono pressoché identici tranne che per qualche riga di testo. Quindi A) è informazione ripetuta, B) se devo modificare qualcosa, 90/100 non mi ricordo di modificare anche la pagina in inglese

### 3) Caricare i file su Poisson
Lavorando da Windows l'unico modo che ho per connettermi in SSH alle macchine del PHC è di utilizzare software di terze parti (nel mio caso, WinSCP). A questo uno ci fa il callo, ma se si riuscisse ad unificare editor di testo e software di upload dei file sarebbe davvero comodo. Questo software fa esattamente questo, tramite dei file batch

## Come funziona
WebsiteCompiler è un editor di testo che, premendo un pulsante, prende tutti i file provenienti da una cartella sorgente e li "compila", ovvero va a sostituire a determinate parole chiave determinati blocchi di testo o stringhe, per andare poi a salvare i file compilati in una cartella di output. 
Il software da la possibilità di creare due volte lo stesso file, uno in un path relativo uguale a quello originale, e uno in una sottocartella "en" del path relativo originale, così che, supponendo che quello che si sta compilando sia un sito internet, basta aggiungere il suffisso "en/" al link per ottenere la versione in inglese. 
Il software offre poi la possibilità di caricare i file sulle macchine del PHC

Esistono due tipi di parole chiave: le parole che identificano dei blocchi di testo, e le parole che identificano delle stringhe variabili (che vengono compilate differentemente in base a quale delle due lingue dovrebbe avere il file che si sta attualmente compilando

### Blocchi di testo
E' possibile inserire un blocco di testo scritto in un altro file usanto la parola chiave £nome_del_blocco£. Il file del blocco dovrà essere salvato in una cartella "blocks" contenuta nella stessa cartella dell'.exe (al primo avvio questa cartella verrà creata automaticamente).
I file devono essere salvati come nome_del_blocco.block. Attualmente non c'è modo di creare un nuovo blocco da dentro il software, per cui bisognerà crearlo manualmente e riavviare il software per aggiornare la lista di blocchi.
La lista di blocchi è situata in basso a sinistra. Facendo doppio click su uno dei blocchi è possibile editarlo direttamente dal software.

### Stringhe variabili
E' possibile inserire stringhe variabili all'interno del file da compilare, che verranno compilate diversamente se il file sta venendo compilato nella prima o nella seconda lingua. Ciò vuol dire che è possibile avere un unico file per una pagina sia in inglese che in italiano,
e basterà scrivere le stringhe "in lingua" (cioè tutto tranne il codice) con delle variabili, e il software si occuperà del resto. Le stringhe variabili sono denotate da $nome_della_variabile$ e sono salvate in una cartella "variables" contenuta nella stessa cartella dell'.exe.
Per creare una variabile basta scrivere $nome_della_variabile_nuova$ in un qualsiasi testo (che sia file della sorgente o che sia un blocco) e fare doppio click sul nome. Questo creerà automaticamente la variabile e aprirà sul pannello a destra delle caselle di testo contenenti
le due versioni (nelle due lingue) della stringa variabile.

### Caricare su Poisson
Per poter usare questo software per caricare i file su Poisson è necessario aver installato il software WinSCP ( https://winscp.net/ ). Una volta fatto ciò, è necessario svolgere le seguenti azioni
- (Da WinSCP) salvare le informazioni di accesso alle macchine del PHC inserendo nome server, nome utente e password nella schermata all'avvio (e salvarle, ovviamente).
- Cliccare col destro sulla sessione appena salvata e andare a "Genera URL sessione/codice"
- Andare in Script e selezionare come formato "File Script"
- Copiare il file script generato sottostante, e salvarlo in un file "script.txt" nella stessa cartella dell'.exe
- Aggiungere a questo script, dopo open e prima di exit, le seguenti due righe di codice
```
cd public_html
synchronize remote -delete %1%
```
- Salvare lo script.

Fatto ciò sarà possibile usare il pulsante Compile & Upload per caricare i file su Poisson
