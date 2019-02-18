# EditableListView_Demo
![アプリスクリーンショット](https://github.com/TN8001/EditableListView_Demo/blob/master/AppImage.png)
## 概要
ListViewのGridViewでセルを編集可能にする ListEditBlockコントロールの使用サンプル
## 特徴
* 出来る限りエクスプローラの詳細モードライクな操作性
* xamlがなるべくシンプルになるような簡便な記述性
* 列ごとに異なるバリデーションを掛けられる
* 編集を確定するまでバインドしたプロパティは変更しない
* ListBoxでも使用可能
## 使い方
デモプロジェクトを参照のこと
## 顛末
同じ様なものを何度か作っているのですが なかなか納得のいくものにならずに困っています

欲しいのはエクスプローラの詳細モード風なものなのですが
DataGridベースだと動作が違いすぎて大変なので
ListViewを編集出来るようにする方が簡単だろうと思います

web上にいくつか作例がありますが 想定が違うものを除くとほぼ一つの元ネタになるようです

[多分元ネタ](https://blogs.msdn.microsoft.com/atc_avalon_team/2006/03/13/editing-in-listview/)

[元ネタをReSharperかけたような感じ 何故か使用例がない](https://github.com/Microsoft/WPF-Samples/tree/master/Sample%20Applications/ExpenseIt/EditBoxControlLibrary)

[日本語のページ](http://pro.art55.jp/?eid=908012)

ぱっと見 まずそうだなぁと思ったらやっぱりメモリリークします
WPFのItemsControlは仮想化まわりがやっかいです

当時と違って今はWeakEventも簡単になりまして**多分**リークは治ったと思いますｗ

色々手を入れて**私の**要求仕様はほぼ満たせました（まだ実際に使ってないのであれですが）

というかEditableListViewの需要は高いと思うのですが皆さんどうしているんでしょうか？？
とっくの昔に自作しているのか それとも買っているのか...

Nugetとかにあればこんなクソコード公開せずに済むのですがw
## ライセンス
CC0 1.0 Universal

[![CC0](http://i.creativecommons.org/p/zero/1.0/88x31.png)](LICENSE)
## 注意事項
* データは架空です（このサンプルではファイルをいじることはありません）
* カスタムコントロールですがテンプレートを変更するようなテストはしていません
* 一切責任は持ちません
## 更新履歴
* 2017/9/21 ver1.0  
* 2017/10/1 ver1.1  
    * ベタ置き用にEditBlockコントロールの追加
    * EditBlock追加のためクラス名調整
* 2017/10/8 ver1.2
    * VirtualizationMode="Recycling" 時にクラッシュしたので大幅見直し

## 謝辞
[ViewModelKit.Fody Copyright (c) 2016-2017, Yves Goergen](http://unclassified.software/source/viewmodelkit)
[MIT](https://github.com/ygoe/ViewModelKit/blob/master/LICENSE.txt)
