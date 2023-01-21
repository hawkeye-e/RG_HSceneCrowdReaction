# RG_HSceneCrowdReaction

This mod is to change the reaction of the surrounding characters who see a H scene in front of them.

## Features
- Allow the Character to have different reaction when seeing a H scene instead of just staying awkward.
- Allow the characters in pair to start H-animation even they are not involved in H scene. The position, speed of H-animation is randomly selected and will update periodically.

## How to trigger H-animation:
Before starting a H scene, get the other characters in pair and make sure they have already had sex with each other (Configurable).



---
Excluded H position list
<table>
  <tr>
    <th>Position</th>
    <th>Exclude Reason</th>
  </tr>
  <tr>
    <td>
      キス, 添い寝キス, キスオナニー, 無理やりキス, <br>
      立ちキス, 立ちキス愛撫, 鏡前抱きつきキス立位, <br>
      ソファーキス騎乗位
    </td>
    <td>Have not figured out the logic of kiss motion</td>
  </tr>
  <tr>
    <td>
      胸弄り, 後ろから胸もみ, しゃがみ胸もみ, <br>
      机上胸もみ, サポート胸もみ, 診察胸いじり, <br>
      診察台胸もみ, 鏡前もたれ胸もみ, パイズリ, <br>
      パイズリ舐め, パイズリ咥え, 立ちパイズリ, <br>
      立ちパイズリ舐め, 椅子パイズリ, 椅子パイズリ舐め, <br>
      寝パイズリ, 胸もみ正常位, 胸もみ騎乗位, <br>
      椅子胸もみ背面座位
    </td>
    <td>Unable to animate multiple animation layers</td>
  </tr>
  <tr>
    <td>
      脱力手コキ, 脱力フェラ
    </td>
    <td>Faintness motion</td>
  </tr>
  <tr>
    <td>
      しゃがみオナニー, ディルドオナニー, <br>
      寝オナニー, 角オナニー, シャワーオナニー, <br>
      椅子オナニー, 風呂オナニー, 机椅子オナニー
    </td>
    <td>Single Masturbation</td>
  </tr>
  </tr>
    <tr>
    <td>
      机スパンキング, 椅子スパンキング
    </td>
    <td>motion require player control</td>
  </tr>
</table>

---
Possible Reaction Type List

<table>
  <tr>
    <th>Condition</th>
    <th>Possible Types</th>
  </tr>
  <tr>
    <td rowspan="2">Serious(まじめ)</td>
    <td>Awkward</td>
  </tr>
  <tr>
    <td>Angry</td>
  </tr>
  <tr>
    <td rowspan="2">Playful(あそび)</td>
    <td>Hurray</td>
  </tr>
  <tr>
    <td>Excited</td>
  </tr>
  <tr>
    <td rowspan="2">Unique(ユニーク)</td>
    <td>Worry</td>
  </tr>
  <tr>
    <td>Happy</td>
  </tr>
  <tr>
    <td>Female's Libido value higher than 50</td>
    <td>Masturbation</td>
  </tr>
  <tr>
    <td>Partner(恋人) involved in H Scene</td>
    <td>Cry</td>
  </tr>
</table>

## Installation
1. Download the plugin from [Releases](https://github.com/hawkeye-e/RG_HSceneCrowdReaction/releases)
2. Extract the zip file and copy to the "BepInEx\plugins" folder in your RG installation directory
