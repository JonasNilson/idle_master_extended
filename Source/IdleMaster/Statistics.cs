namespace IdleMaster
{
    public class Statistics
    {
        private uint sessionMinutesIdled = 0;
        private uint sessionCardIdled = 0;
        private uint remainingCards = 0;

        public uint getSessionMinutesIdled()
        {
            return sessionMinutesIdled;
        }

        public uint getSessionCardIdled()
        {
            return sessionCardIdled;
        }

        public uint getRemainingCards()
        {
            return remainingCards;
        }

        public void setRemainingCards(uint remainingCards)
        {
            this.remainingCards = remainingCards;
        }

        public void checkCardRemaining(uint actualCardRemaining)
        {
            if (actualCardRemaining < remainingCards)
            {
                increaseCardIdled(remainingCards - actualCardRemaining);
                remainingCards = actualCardRemaining;
            }
            else if (actualCardRemaining > remainingCards)
            {
                remainingCards = actualCardRemaining;
            }

        }

        public void increaseCardIdled(uint number)
        {
            Properties.Settings.Default.totalCardIdled += number;
            Properties.Settings.Default.Save();
            sessionCardIdled += number;
        }

        public void increaseMinutesIdled()
        {
            Properties.Settings.Default.totalMinutesIdled++;
            Properties.Settings.Default.Save();
            sessionMinutesIdled++;
        }


    }
}
