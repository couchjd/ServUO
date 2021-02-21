namespace Server.Items
{
	class GMRobe : BaseSuit
	{
		[Constructable]
		public GMRobe()
				: base(AccessLevel.GameMaster, 0x0, 0x204F)
		{
			Name = "GM Robe";
			LootType = LootType.Blessed;
		}

		public GMRobe(Serial serial)
			: base(serial)
		{
		}

		public override void OnDoubleClick(Mobile from)
		{
			if (from.IsPlayer())
			{
				from.SendMessage("This item is to only be used by staff members.");
				Delete();
			}
		}

		public override bool OnEquip(Mobile from)
		{
			if (from.IsPlayer())
			{
				from.SendMessage("This item is to only be used by staff members.");
				Delete();
			}
			return true;
		}

		public override void Serialize(GenericWriter writer)
		{
			base.Serialize(writer);

			writer.Write(0); // version
		}

		public override void Deserialize(GenericReader reader)
		{
			base.Deserialize(reader);

			int version = reader.ReadInt();
		}
	}
}
