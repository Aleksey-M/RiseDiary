namespace UITests

open NUnit.Framework
open canopy.parallell

type MainPageClass () =

    [<SetUp>]
    member this.Setup () =
        ()

    [<Test>]
    member this.CheckMainPageLinks () =
        use browser = functions.start canopy.types.Firefox
        browser.Manage().Window.Maximize()
        functions.url "http://localhost:62346/Index" browser
        functions.sleep 5
        Assert.Pass();
