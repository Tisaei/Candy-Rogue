using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "MyScriptable/Create ActorData")]
public class ActorData : ScriptableObject // プレイヤーに付与するときはレベル1の値を入れる.
{
	public string actorName;
	public int id;
	public int maxHp;
	public int atk;
	public int def;
	public int exp;
	public int gold;
}
