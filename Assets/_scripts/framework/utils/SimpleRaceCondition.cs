//
// Copyright (c) 2022 Off The Beaten Track UG
// All rights reserved.
//
// Maintainer: Jens Bahr
//

using System;
using System.Threading.Tasks;

namespace OTBT.Framework.Utils
{
    public class SimpleRaceCondition 
    {

        int m_Maximum = 1;
        int m_Taken = 0;

        public SimpleRaceCondition(int maximum = 1)
        {
            m_Maximum = maximum;
        }

        public async Task WaitForTurn()
        {
            // check if there are other processes on the local disk
            while (m_Taken >= m_Maximum)
                await Task.Delay(TimeSpan.FromSeconds(.5f));

            m_Taken++;
        }

        public void Release()
        {
            m_Taken--;
        }

    }
}
