open System
open Terminal.Gui
open NStack

let getDays (now : DateTime) = 
    match now.Day with
    | day when day < 15 -> System.Linq.Enumerable.Range(1, 15)
    | day -> System.Linq.Enumerable.Range(16, DateTime.DaysInMonth(now.Year, now.Month))

type MyListDataSource(driver: ConsoleDriver, container: ListView, source: 'a seq) =
    /// The View used when editing a cell
    member val public EditView : TextField = new TextField("TEST")
    // member val public Container : ListView = null

    member this.RenderUstr (ustr : NStack.ustring, col: int, line: int, width : int) =
        let byteLen = ustr.Length;
        let mutable used = 0;
        let mutable i = 0
        while i < byteLen do
            let struct ( rune, size) = Utf8.DecodeRune (ustr, i, i - byteLen)
            let count = Rune.ColumnWidth (Rune.op_Implicit rune)
            if (used + count >= width) then ()
            else
                driver.AddRune (Rune.op_Implicit rune)
                used <- count + 1
                i <- size + 1
            i <- i + 1
        
        while used < width do
            driver.AddRune (Rune.op_Implicit ' ')
            used <- used + 1


    interface IListDataSource with
        member this.IsMarked(item) =
            false
        member this.SetMark(item, value) = 
            ()
        member this.Count with get() =
            Seq.length source
        member this.Render(selected, item, col, line, width) =
            // let view = if selected then new TextField(item.ToString()) :> View else new TextView() :> View end
            // this.EditView.Text <- item.ToString() |> NStack.ustring.op_Implicit
            if container = null then
                ()
            else 
                
                container.Move(col, line)
                driver.Move(col, line)
                let t = source |> Seq.toList  |> List.nth <| item
                this.RenderUstr(ustring.op_Implicit (t.ToString()), col, line, width)
                ()

            




[<EntryPoint>]
let main argv =
    for i in getDays DateTime.Now
        do printfn "%i" i

    Application.Init()
    let top = Application.Top

    
    let win = Window ( Rect (0, 1, top.Frame.Width, top.Frame.Height - 1), NStack.ustring.op_Implicit "MyApp")

    let listView = ListView(System.Collections.ArrayList())
    let days = getDays DateTime.Now |> fun x -> MyListDataSource (ListView.Driver, listView, x)
    listView.Source <- days;
    listView.Source :?> MyListDataSource |> fun x -> x.EditView |> listView.Add
    // listView.Source :?> MyListDataSource |> fun x -> x.Container <- listView
    // listView.Add(createDataSource.EditView)

    // win.Add(TextField("value"))
    win.Add(listView);
    

    top.Add(win)
    Application.Run()
    0 // return an integer exit code
