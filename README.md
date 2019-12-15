# アセンブリ名を書き換えるMono.Cecilの機能検証プロジェクト

post/Properties/launchSettings.json
からデバッグ時に渡すコマンドライン引数を適当に弄ってください。

# 再現手順

git cloneして出来たディレクトリが現在のワーキングディレクトリだと思います。

```powershell:powershell
cd E
dotnet run -c Release
```

コンソールに-960と出力されるはずです。
この段階で `E/bin/Release/netcoreapp3.1/` にあるA.dll, B.dll, C.dll, D.dll, E.dllをILSpyで読んで見ましょう。

`[A]A.Class1` とかそんな感じの記述が見られるはずです。

```powershell:powershell
cd ../post
dotnet run rewrite "../E/bin/Release/netcoreapp3.1/"
```

処理対象のDLL全てのアセンブリ名が書き換わります。

現在のディレクトリはpostだと思います。

```powershell:powershell
cd "../E/bin/Release/netcoreapp3.1"
Rename-Item -Path A.dll -NewName aa.dll -Force
Rename-Item -Path B.dll -NewName aaa.dll -Force
Rename-Item -Path C.dll -NewName aaaa.dll -Force
Rename-Item -Path D.dll -NewName aaaaa.dll -Force
```

そしてE.deps.jsonを下記内容に置換してください。ここで私は30分以上時間を溶かしました。

```json:E.deps.json
{
  "runtimeTarget": {
    "name": ".NETCoreApp,Version=v3.1",
    "signature": ""
  },
  "compilationOptions": {},
  "targets": {
    ".NETCoreApp,Version=v3.1": {
      "E/1.0.0": {
        "dependencies": {
          "aaaaa": "1.0.0"
        },
        "runtime": {
          "E.dll": {}
        }
      },
      "A/1.0.0": {
        "runtime": {
          "aa.dll": {}
        }
      },
      "B/1.0.0": {
        "dependencies": {
          "aa": "1.0.0"
        },
        "runtime": {
          "aaa.dll": {}
        }
      },
      "C/1.0.0": {
        "dependencies": {
          "aa": "1.0.0"
        },
        "runtime": {
          "aaaa.dll": {}
        }
      },
      "D/1.0.0": {
        "dependencies": {
          "aaa": "1.0.0",
          "aaaa": "1.0.0"
        },
        "runtime": {
          "aaaaa.dll": {}
        }
      }
    }
  },
  "libraries": {
    "E/1.0.0": {
      "type": "project",
      "serviceable": false,
      "sha512": ""
    },
    "A/1.0.0": {
      "type": "project",
      "serviceable": false,
      "sha512": ""
    },
    "B/1.0.0": {
      "type": "project",
      "serviceable": false,
      "sha512": ""
    },
    "C/1.0.0": {
      "type": "project",
      "serviceable": false,
      "sha512": ""
    },
    "D/1.0.0": {
      "type": "project",
      "serviceable": false,
      "sha512": ""
    }
  }
}
```

さて、ここまでお膳立てしてからまたpowershellです。
カレントワーキングディレクトリは `E/bin/Releases/netcoreapp3.1/` のはずです。

```powershell:powershell
./E.exe
cd ../../../
dotnet run -c Release --no-build
```

-960と2回出力されるはずです。

この段階でaa.dll, aaa.dll, aaaa.dll, aaaaa.dllなどをILSpyで読んでみましょう。
適切に書き換えられているはずです。

以上です。

# 何をしているのか

アセンブリ名と共にモジュール名を書き換えます。
モジュールの参照するアセンブリ名(AssemblyReferences)もそれに応じて適切に書き換えてください。

# ドハマリポイント

## その1 AssemblyReferences書き換え忘れ

アセンブリにはモジュールが最低1つ含まれています。
モジュールはそのメタデータとして依存するアセンブリの名前とかバージョンとかハッシュ値とか公開鍵とかの組を持っています。
この依存アセンブリ名を適切に書き換えないと「書き換え前の既にこの世に存在しなくなったアセンブリに含まれる要素」を参照しだすようになり、プログラムはすぐ死にます。

## その2 アセンブリ検索

アセンブリ検索の仕組みが.net coreだとめんどくさいことになっていましたので、気をつけてください。
[deps.jsonファイルに関する詳しいお話がこちらに書かれています。](http://yfakariya.blogspot.com/2017/03/net-core.html)

.deps.jsonというファイルがあります。
targets以下にdependenciesとruntimeを含んでいるものがあるじゃないですか。
 
 - dependencies
  - 依存しているアセンブリ名とバージョンのペア
  - アセンブリ名書き換え必須
 - runtime
  - アセンブリ検索の際に調べるファイル名
  - 該当するファイルが存在しないとIOExceptionを投げて死ぬ

これらをアセンブリ名書き換えに応じて適切に書き換えないと駄目なのですよね。

# 注意喚起

https://stackoverflow.com/questions/13681714/how-to-change-assembly-name-of-dll-pragmatically-using-mono-cecil

上記解答は使い物にならないので注意。

# VSCodeが使い物にならない

extern aliasをまともに扱えないVSCodeくんちゃんさぁ……（呆れ）