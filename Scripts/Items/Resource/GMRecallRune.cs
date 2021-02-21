using System;
using Server.Network;
using Server.Prompts;
using Server.Multis;
using Server.Regions;

namespace Server.Items
{
	public class GMRecallRune : Item
	{
		private string m_Description;
		private bool m_Marked;
		private Point3D m_Target;
		private Map m_TargetMap;

		[CommandProperty( AccessLevel.GameMaster, AccessLevel.GameMaster )]
		public string Description
		{
			get { return m_Description; }
			set { m_Description = value; InvalidateProperties(); }
		}

		[CommandProperty( AccessLevel.GameMaster, AccessLevel.GameMaster )]
		public bool Marked
		{
			get { return m_Marked; }
			set { if ( m_Marked != value ) { m_Marked = value; CalculateHue(); InvalidateProperties(); } }
		}

		[CommandProperty( AccessLevel.GameMaster, AccessLevel.GameMaster )]
		public Point3D Target
		{
			get { return m_Target; }
			set { m_Target = value; }
		}

		[CommandProperty( AccessLevel.GameMaster, AccessLevel.GameMaster )]
		public Map TargetMap
		{
			get { return m_TargetMap; }
			set { if ( m_TargetMap != value ) { m_TargetMap = value; CalculateHue(); InvalidateProperties(); } }
		}

		[Constructable]
		public GMRecallRune() : base( 0x1F14 )
		{
			CalculateHue();
			Name = "GM Recall Rune";
		}

		private void CalculateHue()
		{
			if ( !m_Marked ) Hue = 0;
			else if ( m_TargetMap == Map.Trammel ) Hue = 50;
			else if ( m_TargetMap == Map.Felucca ) Hue = 0;
			else if ( m_TargetMap == Map.Ilshenar ) Hue = 1102;
			else if ( m_TargetMap == Map.Malas ) Hue = 1102;
			else if ( m_TargetMap == Map.Tokuno ) Hue = 1102;
		}

		public void Mark( Mobile m )
		{
			m_Marked = true;
			bool setDesc = false;
			m_Target = m.Location;
			m_TargetMap = m.Map;
			if( !setDesc ) m_Description = BaseRegion.GetRuneNameFor( Region.Find( m_Target, m_TargetMap ) );
			CalculateHue();
			InvalidateProperties();
		}

		private const string RuneFormat = "a recall rune for {0}";

		public override void GetProperties( ObjectPropertyList list )
		{
			base.GetProperties( list );
			if ( m_Marked )
			{
				string desc;
				if ( (desc = m_Description) == null || (desc = desc.Trim()).Length == 0 ) desc = "an unknown location";
				if ( m_TargetMap == Map.Tokuno ) list.Add(1063259, RuneFormat, desc );
				else if ( m_TargetMap == Map.Malas ) list.Add(1060804, RuneFormat, desc );
				else if ( m_TargetMap == Map.Felucca ) list.Add(1060805, RuneFormat, desc );
				else if ( m_TargetMap == Map.Trammel ) list.Add(1060806, RuneFormat, desc );
				else list.Add("{0} ({1})", String.Format( RuneFormat, desc ), m_TargetMap );
			}
		}

		public override void OnDoubleClick( Mobile from )
		{
			int number;
			if ( !IsChildOf( from.Backpack ) ) number = 1042001;
			else if ( m_Marked )
			{
				number = 501804;
				from.Prompt = new RenamePrompt( this );
			}
			else number = 501805;
			from.SendLocalizedMessage( number );
		}

		private class RenamePrompt : Prompt
		{
			private GMRecallRune m_Rune;
			public RenamePrompt( GMRecallRune rune )
			{
				m_Rune = rune;
			}
			public override void OnResponse( Mobile from, string text )
			{
				m_Rune.Description = text;
				from.SendLocalizedMessage( 1010474 );
			}
		}

		public GMRecallRune(Serial serial) : base(serial){}
		public override void Serialize( GenericWriter writer )
		{
			base.Serialize( writer );
			writer.Write( (int) 0 );
			writer.Write( (string) m_Description );
			writer.Write( (bool) m_Marked );
			writer.Write( (Point3D) m_Target );
			writer.Write( (Map) m_TargetMap );
		}
		public override void Deserialize( GenericReader reader )
		{
			base.Deserialize( reader );
			int version = reader.ReadInt();
			m_Description = reader.ReadString();
			m_Marked = reader.ReadBool();
			m_Target = reader.ReadPoint3D();
			m_TargetMap = reader.ReadMap();
			CalculateHue();
		}
	}
}